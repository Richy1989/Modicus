using System;
using System.Device.I2c;
using System.Diagnostics;
using Modicus.Manager.Interfaces;
using Modicus.Sensor;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command we can create new I2C Sensor devices
    internal class CmdCreateI2CSensor : BaseCommand
    {
        private readonly IBusDeviceManager busDeviceManager;

        public CmdCreateI2CSensor(ISettingsManager settingsManager, IBusDeviceManager busDeviceManager)
        {
            this.busDeviceManager = busDeviceManager;
            Topic = settingsManager.GlobalSettings.CommandSettings.CreateI2CSensor;
        }

        public void Execute(CmdCreateI2CSensorData content)
        {
            base.mreExecute.WaitOne();

            var sensorType = busDeviceManager.SupportedSensors[content.SensorType];

            try
            {
                if (sensorType != null)
                {
                    BaseI2cSensor sensor = (BaseI2cSensor)Activator.CreateInstance(((Type)sensorType));
                    sensor.SclPin = content.SclPin;
                    sensor.SdaPin = content.SdaPin;
                    sensor.MeasurementInterval = content.MeasurementInterval;
                    sensor.BusID = content.BusID;
                    sensor.I2cBusSpeed = (I2cBusSpeed)content.I2cBusSpeed;
                    sensor.DeviceAddress = content.DeviceAddress;

                    busDeviceManager.AddSensor(sensor);
                    busDeviceManager.StartSensor(sensor);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in create i2c sensor command: {ex.Message}");
            }

            base.mreExecute.Set();
        }

        //Execute the command
        public new void Execute(string content)
        {
            CmdCreateI2CSensorData data = (CmdCreateI2CSensorData)JsonConvert.DeserializeObject(content, typeof(CmdCreateI2CSensorData));
            Execute(data);
            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdCreateI2CSensorData
    {
        public string SensorType { get; set; }
        public string Name { get; set; }
        public int MeasurementInterval { get; set; }
        public int SclPin { get; set; }
        public int SdaPin { get; set; }
        public int BusID { get; set; }
        public int I2cBusSpeed { get; set; }
        public int DeviceAddress { get; set; }
    }
}