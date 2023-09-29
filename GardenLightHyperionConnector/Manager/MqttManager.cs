using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Modicus.Commands;
using Modicus.Interfaces;
using Modicus.MQTT;
using Modicus.MQTT.Interfaces;
using Modicus.Settings;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;

namespace Modicus.Manager
{
    internal class MqttManager : IPublishMqtt, ICommandCapable
    {
        private MqttClient mqtt;
        private bool closeConnection = false;
        private CancellationToken token;
        private ModicusStartupManager modicusStartupManager;
        private GlobalSettings globalSettings;
        private bool restartService = false;

        //Make sure only one thread at the time can work with the mqtt service
        private ManualResetEvent mreMQTT = new(true);

        public MainMqttMessage MainMqttMessage { get; set; }
        public StateMessage State { get; set; }
        public IDictionary SubscribeTopics { get; private set; }

        public MqttManager(ModicusStartupManager modicusStartupManager, CancellationToken token)
        {
            this.globalSettings = modicusStartupManager.GlobalSettings;
            this.modicusStartupManager = modicusStartupManager;
            this.token = token;
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
            mqtt?.Close();
            mqtt?.Dispose();

            try
            {
                EstablishConnection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR connecting MQTT: {ex.Message}");
            }

            if (restartService)
            {
                ResubscribeToNewClientID();
            }

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
            restartService = false;
        }

        /// <summary>
        /// Send the MQTT messages
        /// </summary>
        public void StartSending()
        {
            while (!token.IsCancellationRequested)
            {
                if (!restartService && mqtt != null && mqtt.IsConnected)
                {

                    mreMQTT.WaitOne();
                    //Set current time
                    MainMqttMessage.Time = DateTime.UtcNow;
                    State.Uptime = DateTime.UtcNow - modicusStartupManager.startupTime;
                    State.UptimeSec = State.Uptime.TotalSeconds;

                    Publish("STATE", JsonConvert.SerializeObject(State));
                    Publish("SENSOR", JsonConvert.SerializeObject(MainMqttMessage));
                    
                    mreMQTT.Set();

                    Thread.Sleep(globalSettings.MqttSettings.SendInterval);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }
        /// <summary>
        /// Create new MQTT Client and start a connectrion with necessary subscriptions
        /// </summary>
        private void EstablishConnection()
        {
            mqtt = new MqttClient(globalSettings.MqttSettings.MqttHostName);
            var ret = mqtt.Connect(globalSettings.MqttSettings.MqttClientID, globalSettings.MqttSettings.MqttUserName, globalSettings.MqttSettings.MqttPassword);

            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"++++ ERROR connecting: {ret} ++++");
                mqtt.Disconnect();
                return;
            }

            mqtt.ConnectionClosed += (s, e) =>
            {
                if (!closeConnection)
                {
                    InitializeMQTT();
                }
            };

            Debug.WriteLine($"++++ MQTT connecting successful: {ret} ++++");
        }

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

            if (command.GetType() == typeof(CmdMQTTClientID))
            {
                command.CommandRaisedEvent += Command_MQTTClientIDCommandRaisedEvent;
            }

        }

        /// <summary>
        /// The Event callback for a MQTT Client ID Change command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_MQTTClientIDCommandRaisedEvent(object sender, GardenLightHyperionConnector.EventArgs.CommandRaisedEventArgs e)
        {
            Debug.WriteLine($"++++ New Client ID, restart service ++++");
            restartService = true;

            mreMQTT.WaitOne();

            mqtt?.Disconnect();
            mqtt?.Close();

            mreMQTT.Set();
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
