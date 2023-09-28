using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GardenLightHyperionConnector.Interfaces;
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
        private string ip;
        private string clientID;
        private string user;
        private string password;
        private CancellationToken token;
        private ModicusStartupManager modicusStartupManager;
        private GlobalSettings globalSettings;

        public MainMqttMessage MainMqttMessage { get; set; }
        public StateMessage State { get; set; }
        public IDictionary SubscribeTopics { get; }

        public MqttManager(ModicusStartupManager modicusStartupManager, string clientID, CancellationToken token)
        {
            this.globalSettings = modicusStartupManager.GlobalSettings;
            this.clientID = clientID;
            this.modicusStartupManager = modicusStartupManager;
            this.token = token;
            SubscribeTopics = new Hashtable();
            MainMqttMessage = new MainMqttMessage();
            State = new StateMessage();
            State.WiFi = new WiFiMessage();
        }

        public void Connect(string ip, string user, string password)
        {
            this.ip = ip;
            this.user = user;
            this.password = password;

            closeConnection = false;
        }

        public void InitializeMQTT()
        {
            EstablishConnection();

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
        }

        private void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine($"MQTT Command Received:\nTopic:\n{e.Topic}\nContent:\n{Encoding.UTF8.GetString(e.Message, 0, e.Message.Length)}");

            var subscriber = (IMqttSubscriber)SubscribeTopics[e.Topic];
            subscriber?.Execute(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
        }

        public void RegisterCommand(ICommand command)
        {
            AddSubcriber(command as IMqttSubscriber);
        }

        public void AddSubcriber(IMqttSubscriber subscriber)
        {
            var topic = $"{clientID}/cmd/{subscriber.Topic}";
            SubscribeTopics.Add(topic, subscriber);
        }

        private void EstablishConnection()
        {
            mqtt = new MqttClient(ip);
            var ret = mqtt.Connect(clientID, user, password);

            if (ret != MqttReasonCode.Success)
            {
                Debug.WriteLine($"ERROR connecting: {ret}");
                mqtt.Disconnect();
                return;
            }

            mqtt.ConnectionClosed += (s, e) =>
            {
                if (!closeConnection)
                {
                    EstablishConnection();
                }
            };

            Debug.WriteLine($"MQTT connecting successful: {ret}");
        }

        public void Publish(string topic, string message)
        {
            SendMessage(topic, Encoding.UTF8.GetBytes(message));
        }


        public void SendMessage(string topic, byte[] message)
        {
            string to = $"{clientID}/{topic}";
            mqtt.Publish(to, message);
        }

        public void StartSending()
        {
            while (!token.IsCancellationRequested)
            {
                //foreach (var message in Messages.Values)
                //{
                //    string jsonString = nanoFramework.Json.JsonConvert.SerializeObject(message);
                //    Publish(((IMessageBase)message).Topic, jsonString);
                //}

                //Set current time
                MainMqttMessage.Time = DateTime.UtcNow;
                State.Uptime = DateTime.UtcNow - modicusStartupManager.startupTime;
                State.UptimeSec = State.Uptime.TotalSeconds;


                Publish("STATE", JsonConvert.SerializeObject(State));
                Publish("SENSOR", JsonConvert.SerializeObject(MainMqttMessage));
                Thread.Sleep(globalSettings.MqttSettings.SendInterval);
            }
        }
    }
}
