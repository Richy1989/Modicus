﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Modicus.Commands;
using Modicus.Commands.Interfaces;
using Modicus.EventArgs;
using Modicus.Manager.Interfaces;
using Modicus.MQTT;
using Modicus.MQTT.Interfaces;
using Modicus.OutputDevice;
using Modicus.Settings;
using Modicus.WiFi;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;

namespace Modicus.Manager
{
    internal class MqttManager : BaseOutputDevice, IMqttManager, IPublishMqtt, ICommandCapable
    {
        //Make sure only one thread at the time can work with the mqtt service
        private readonly ManualResetEvent mreMQTT = new(true);

        private MqttClient mqtt;
        private readonly CancellationToken token;
        private readonly GlobalSettings globalSettings;
        private bool resubscribeAll = false;
        private bool stopService = false;
        private bool enableAutoRestart = true;
        public IDictionary SubscribeTopics { get; private set; }

        /// <summary>Initializes a new MQTT Manager.</summary>
        /// <param name="modicusStartupManager"></param>
        /// <param name="token"></param>
        public MqttManager(IOutputManager outputManager, ISettingsManager settingsManager, ITokenManager tokenManager) : base(outputManager)
        {
            this.globalSettings = settingsManager.GlobalSettings;
            this.token = tokenManager.Token;
            SubscribeTopics = new Hashtable();
        }

        /// <summary>Initialize the MQTT Service, repeat this function periodically to make sure we always reconnect.</summary>
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

            outputManager.RegisterOutputDevice(this);
        }

        public void StopMqtt(bool preventAutoRestart = true)
        {
            //make sure we do not restart the service automatically on stopping
            stopService = preventAutoRestart;

            //Set this variable to ensure a gracefull startup when we resart the service
            resubscribeAll = true;

            enableAutoRestart = false;

            mreMQTT.WaitOne();

            //mqtt?.Disconnect();
            mqtt?.Close();
            mqtt?.Dispose();

            outputManager.RemoveOutputDevice(this);

            mreMQTT.Set();
        }

        /// <summary>.Create new MQTT Client and start a connectrion with necessary subscriptions.</summary>
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
                if (!stopService)
                    InitializeMQTT();
            };

            enableAutoRestart = true;

            Debug.WriteLine($"++++ MQTT connecting successful: {ret} ++++");
            return true;
        }

        /// <summary>Event callback when a subscribed message is received.</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">MqttMsgPublishEventArgs</param>
        private void Mqtt_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine($"++++ MQTT Command Received:\nTopic:\n{e.Topic}\nContent:\n{Encoding.UTF8.GetString(e.Message, 0, e.Message.Length)} ++++");

            var subscriber = (ICommand)SubscribeTopics[e.Topic];
            subscriber?.Execute(Encoding.UTF8.GetString(e.Message, 0, e.Message.Length));
        }

        /// <summary>Register new command to the MQTT Manager.</summary>
        /// <param name="command"></param>
        public void RegisterCommand(ICommand command)
        {
            AddSubcriber(command as ICommand);

            if (command.GetType() == typeof(CmdMqttClientID))
            {
                command.CommandRaisedEvent += Command_MQTTClientIDCommandRaisedEvent;
            }
        }

        /// <summary>Publish a message to the MQTT broker.</summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        public void Publish(string topic, string message) => SendMessage(topic, Encoding.UTF8.GetBytes(message));

        /// <summary>Publishes all data from modicus on the output device.</summary>
        /// <param name="state">The state.</param>
        /// <param name="sensorData">The sensor data.</param>
        public override void PublishAll(string state, string sensorData)
        {
            Publish("STATE", state);
            Publish("SENSOR", sensorData);
        }

        /// <summary>The Event callback for a MQTT Client ID Change command.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_MQTTClientIDCommandRaisedEvent(object sender, CommandRaisedEventArgs e)
        {
            Debug.WriteLine($"++++ New Client ID, restart service ++++");

            //Stop MQTT but set ready for auto restart.
            StopMqtt(false);
        }

        /// <summary>Add new subscribing service to a specified command topic.</summary>
        /// <param name="subscriber"></param>
        private void AddSubcriber(ICommand subscriber)
        {
            var topic = $"{globalSettings.MqttSettings.MqttClientID}/cmd/{subscriber.Topic}";
            SubscribeTopics.Add(topic, subscriber);
        }

        /// <summary>Send a the message to publish.</summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        private void SendMessage(string topic, byte[] message)
        {
            if (mqtt == null || !mqtt.IsConnected) return;

            string to = $"{globalSettings.MqttSettings.MqttClientID}/{topic}";
            mqtt.Publish(to, message);
        }

        /// <summary>Resubsribe all commands again after we have a new MQTT client ID.</summary>
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
                AddSubcriber(command);
            }
        }
    }
}