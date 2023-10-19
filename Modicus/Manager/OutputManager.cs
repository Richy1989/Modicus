using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Modicus.Extensions;
using Modicus.Manager.Interfaces;
using Modicus.MQTT;
using Modicus.OutputDevice.Interface;
using Modicus.Sensor.Measurement;
using Modicus.WiFi;
using nanoFramework.Json;

namespace Modicus.Manager
{
    internal class OutputManager : IOutputManager
    {
        public IDictionary OutputData { get; set; } = new Hashtable();
        public IList OutputDevices { get; set; } = new ArrayList();

        private readonly ITokenManager tokenManager;
        private readonly ISettingsManager settingsManager;
        private readonly ManualResetEvent mre = new(true);

        /// <summary>Initializes a new instance of the <see cref="OutputManager"/> class.</summary>
        /// <param name="settingsManager">The settings manager.</param>
        /// <param name="tokenManager">The token manager.</param>
        public OutputManager(ISettingsManager settingsManager, ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
            this.settingsManager = settingsManager;

            Thread sendThread = new(StartSending);
            sendThread.Start();
        }

        /// <summary>Adds the output device.</summary>
        /// <param name="device">The device.</param>
        public void RegisterOutputDevice(IOutputDevice device)
        {
            if (OutputDevices.Contains(device))
                return;

            OutputDevices.Add(device);
        }

        /// <summary>Removes the output device..</summary>
        /// <param name="device">The device.</param>
        public void RemoveOutputDevice(IOutputDevice device)
        {
            if (!OutputDevices.Contains(device))
                return;

            OutputDevices.Remove(device);
        }

        /// <summary>Purges the measurement data from the OutputData.</summary>
        /// <param name="measurement">The measurement.</param>
        public void PurgeMeasurementData(BaseMeasurement measurement)
        {
            if (measurement == null)
                return;

            mre.WaitOne();

            if (OutputData.Contains(measurement.MeasurmentCategory))
            {
                IDictionary outDta = (Hashtable)OutputData[measurement.MeasurmentCategory];

                var method = measurement.GetType().GetMethods();

                foreach (var item in method)
                {
                    if (item.Name.StartsWith("get"))
                    {
                        string name = item.Name.Split('_')[1];

                        if (outDta.Contains(name))
                            outDta.Remove(name);
                    }
                }
            }

            mre.Set();
        }

        /// <summary>Adds new measurment data.</summary>
        /// <param name="measurement">The measurement.</param>
        public void AddMeasurementData(BaseMeasurement measurement)
        {
            measurement.Time = DateTime.UtcNow;
            mre.WaitOne();

            if (!OutputData.Contains(measurement.MeasurmentCategory))
            {
                OutputData.Add(measurement.MeasurmentCategory, new Hashtable());
            }

            IDictionary outDta = (Hashtable)OutputData[measurement.MeasurmentCategory];

            var method = measurement.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ParseMethods(outDta, method, measurement);

            mre.Set();
        }

        /// <summary>Parses the methods of the BeaseMeasurement and adds them to the OutputData dictionary.</summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="method">The method.</param>
        /// <param name="instance">The instance.</param>
        private void ParseMethods(IDictionary dict, MethodInfo[] method, BaseMeasurement instance)
        {
            foreach (var item in method)
            {
                if (item.Name.StartsWith("get") && !item.Name.Contains(nameof(instance.MeasurmentCategory)))
                {
                    string name = item.Name.Split('_')[1];
                    var value = item.Invoke(instance, null);

                    if (!dict.Contains(name))
                        dict.Add(name, value);
                    else
                        dict[name] = value;
                }
            }
        }

        /// <summary>Creates a json string of the saved measurement data.</summary>
        /// <returns>Measurement Data JSON string.</returns>
        public string GetJsonString()
        {
            mre.WaitOne();
            string json = JsonCreator((Hashtable)OutputData);
            mre.Set();
            return json;
        }

        /// <summary>Recursivly creates a json string out of the outdata.</summary>
        /// <param name="dictionary">The dictionary.</param>
        private string JsonCreator(Hashtable dictionary)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("{");
            try
            {
                bool isFirst = true;
                foreach (string dataKey in dictionary.Keys)
                {
                    object dataValue = dictionary[dataKey];

                    if (!isFirst)
                        stringBuilder.Append(", ");

                    stringBuilder.Append(string.Format("\"{0}\"", dataKey));
                    stringBuilder.Append(": ");

                    if (dataValue is Hashtable subDictionary)
                    {
                        stringBuilder.Append(JsonCreator(subDictionary));
                    }
                    else if (dataValue.IsNumber())
                    {
                        stringBuilder.Append(dataValue);
                    }
                    else
                    {
                        stringBuilder.Append(string.Format("\"{0}\"", dataValue));
                    }
                    isFirst = false;
                }
            }
            catch (Exception ex)
            {
                //ToDo: This exception shoud not happen, had problems here, maybe remove in the future after further testing
                Debug.WriteLine($"Error in Json Creator in OutManager: {ex.Message}");
            }

            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        /// <summary>Starts the sending.</summary>
        public void StartSending()
        {
            var token = tokenManager.Token;

            while (!token.IsCancellationRequested)
            {
                StateMessage stateMessage = new()
                {
                    Uptime = DateTime.UtcNow - settingsManager.GlobalSettings.StartupTime,
                    UptimeSec = (DateTime.UtcNow - settingsManager.GlobalSettings.StartupTime).TotalSeconds
                };

                stateMessage.WiFi.SSId = settingsManager.GlobalSettings.WifiSettings.Ssid;
                stateMessage.WiFi.IPAddress = Wireless80211.GetIP();
                stateMessage.WiFi.BSSId = Wireless80211.GetPhysicalAddress();

                foreach (IOutputDevice devicedevice in OutputDevices)
                {
                    devicedevice.PublishAll(JsonConvert.SerializeObject(stateMessage), GetJsonString());
                }

                Thread.Sleep(settingsManager.GlobalSettings.SendInterval);
            }
        }
    }
}