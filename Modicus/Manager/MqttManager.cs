using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Modicus.Commands;
using Modicus.Interfaces;
using Modicus.MQTT;
using Modicus.Settings;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using Modicus.EventArgs;
using Modicus.Commands.Interfaces;
using Modicus.MQTT.Interfaces;

namespace Modicus.Manager
{
    internal class MqttManager : IMqttManager, IPublishMqtt
    {
        private MqttClient mqtt;
        private readonly CancellationToken token;
        private readonly GlobalSettings globalSettings;
        private bool resubscribeAll = false;
        private bool stopService = false;
        private bool enableAutoRestart = true;

        //Make sure only one thread at the time can work with the mqtt service
        private readonly ManualResetEvent mreMQTT = new(true);
        private readonly ISettingsManager settingsManager;
        private Thread mqttThread;

        public MainMqttMessage MainMqttMessage { get; set; }
        public StateMessage State { get; set; }
        public IDictionary SubscribeTopics { get; private set; }

        /// <summary>
        /// Initializes a new MQTT Manager
        /// </summary>
        /// <param name="modicusStartupManager"></param>
        /// <param name="token"></param>
        public MqttManager(ISettingsManager settingsManager, ITokenManager tokenManager)
        {
            this.globalSettings = settingsManager.GlobalSettings;
            this.token = tokenManager.Token;
            this.settingsManager = settingsManager;
            SubscribeTopics = new Hashtable();
            MainMqttMessage = new MainMqttMessage();
            State = new StateMessage();
            State.WiFi = new WiFiMessage();
        }

        /// <summary>
        /// Initialize the MQTT Service, repeat this function periodically to make sure we always reconnect.
        /// </summary>
        public void InitializeMQTT()
        {
            if (mqtt != null && mqtt.IsConnected)
            {
                Debug.WriteLine("++++ MQTT is already running. No need to start. ++++");
                return;
            }

            stopService = false;
            mqtt?.Close();
            mqtt?.Dispose();

            bool autoRestartNeeded = false;
            do
            {
                autoRestartNeeded = false;
                try
                {
                    if (!EstablishConnection())
                    {
                        if (enableAutoRestart)
                            autoRestartNeeded = true;
                        else
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR connecting MQTT: {ex.Message}");

                    if (enableAutoRestart)
                        autoRestartNeeded = true;
                }
                if (autoRestartNeeded)
                    Thread.Sleep(1000);
            }
            while (autoRestartNeeded && !token.IsCancellationRequested && !stopService);


            if (resubscribeAll)
                ResubscribeToNewClientID();

            string[] topics = new string[SubscribeTopics.Keys.Count];
            MqttQoSLevel[] level = new MqttQoSLevel[SubscribeTopics.Keys.Count];

            int i = 0;
            foreach (var item in SubscribeTopics.Keys)
            {
                topics[i] = (string)item;
                level[i++] = MqttQoSLevel.ExactlyOnce;
            }

            if (topics.Length > 0)
            {
                mqtt.Subscribe(topics, level);
                mqtt.MqttMsgPublishReceived += Mqtt_MqttMsgPublishReceived;
            }
            resubscribeAll = false;

            mqttThread = new(new ThreadStart(StartSending));
            mqttThread.Start();
        }

        public void StopMqtt(bool preventAutoRestart = true)
        {
            //make sure we do not restart the service automatically on stopping
            stopService = preventAutoRestart;

            //Set this variable to ensure a gracefull startup when we resart the service
            resubscribeAll = true;

            enableAutoRestart = false;
            
            mreMQTT.WaitOne();

            mqtt?.Disconnect();
            mqtt?.Close();
            mqtt?.Dispose();

            mreMQTT.Set();
        }

        /// <summary>
        /// Send the MQTT messages
        /// </summary>
        public void StartSending()
        {
            while (!token.IsCancellationRequested && !resubscribeAll)
            {
                if (!resubscribeAll && mqtt != null && mqtt.IsConnected)
                {
                    mreMQTT.WaitOne();
                    //Set current time
                    MainMqttMessage.Time = DateTime.UtcNow;
                    State.Uptime = DateTime.UtcNow - settingsManager.GlobalSettings.StartupTime;
                    State.UptimeSec = State.Uptime.TotalSeconds;
                    State.WiFi.SSId = settingsManager.GlobalSettings.WifiSettings.Ssid;

                    Publish("STATE", JsonConvert.SerializeObject(State));
                    Publish("SENSOR", JsonConvert.SerializeObject(MainMqttMessage));

                    mreMQTT.Set();

                    Thread.Sleep(globalSettings.MqttSettings.SendInterval);
                }
                else
                {
                    //When no connection try every 5 seconds to send again
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }

        /// <summary>
        /// Create new MQTT Client and start a connectrion with necessary subscriptions
        /// </summary>
        private bool EstablishConnection()
        {
            mqtt = new MqttClient(globalSettings.MqttSettings.MqttHostName, globalSettings.MqttSettings.MqttPort, secure: false, null, null, MqttSslProtocols.None);
            var ret = mqtt.Connect(globalSettings.MqttSettings.MqttClientID, globalSettings.MqttSettings.MqttUserName, globalSettings.MqttSettings.MqttPassword);

            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"++++ ERROR connecting: {ret} ++++");
                mqtt.Disconnect();
                return false;
            }

            mqtt.ConnectionClosed += (s, e) =>
            {
                if(!stopService)
                    InitializeMQTT();
            };

            enableAutoRestart = true;
            
            Debug.WriteLine($"++++ MQTT connecting successful: {ret} ++++");
            return true;
        }

        /// <summary>
        /// Event callback when a subscribed message is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine($"++++ MQTT Command Received:\nTopic:\n{e.Topic}\nContent:\n{Encoding.UTF8.GetString(e.Message, 0, e.Message.Length)} ++++");

            var subscriber = (ICommand)SubscribeTopics[e.Topic];
            subscriber?.Execute(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
        }

        /// <summary>
        /// Register new command to the MQTT Manager
        /// </summary>
        /// <param name="command"></param>
        public void RegisterCommand(ICommand command)
        {
            AddSubcriber(command as ICommand);

            if (command.GetType() == typeof(CmdMqttClientID))
            {
                command.CommandRaisedEvent += Command_MQTTClientIDCommandRaisedEvent;
            }
        }

        /// <summary>
        /// The Event callback for a MQTT Client ID Change command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_MQTTClientIDCommandRaisedEvent(object sender, CommandRaisedEventArgs e)
        {
            Debug.WriteLine($"++++ New Client ID, restart service ++++");

            //Stop MQTT but set ready for auto restart. 
            StopMqtt(false);
        }

        /// <summary>
        /// Add new subscribing service to a specified command topic
        /// </summary>
        /// <param name="subscriber"></param>
        private void AddSubcriber(ICommand subscriber)
        {
            var topic = $"{globalSettings.MqttSettings.MqttClientID}/cmd/{subscriber.Topic}";
            SubscribeTopics.Add(topic, subscriber);
        }

        /// <summary>
        /// Publish a message to the MQTT broker
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public void Publish(string topic, string message) => SendMessage(topic, Encoding.UTF8.GetBytes(message));

        /// <summary>
        /// Send a the message to publish
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        private void SendMessage(string topic, byte[] message)
        {
            string to = $"{globalSettings.MqttSettings.MqttClientID}/{topic}";
            mqtt.Publish(to, message);
        }

        /// <summary>
        /// Resubsribe all commands again after we have a new MQTT client ID
        /// </summary>
        private void ResubscribeToNewClientID()
        {
            IDictionary newSubscribers = new Hashtable();

            foreach (var item in SubscribeTopics.Keys)
            {
                newSubscribers.Add(item, SubscribeTopics[item]);
            }

            SubscribeTopics.Clear();

            foreach (ICommand command in newSubscribers.Values)
            {
                //RegisterCommand(command);
                AddSubcriber(command);
            }
        }
    }
}