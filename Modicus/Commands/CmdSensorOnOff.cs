using System;
using System.Diagnostics;
using Modicus.Manager.Interfaces;
using nanoFramework.Json;

namespace Modicus.Commands
{
    //With this command the ;QTT Service can be turned on and off
    internal class CmdSensorOnOff : BaseCommand
    {
        private readonly IBusDeviceManager busDeviceManager;

        public CmdSensorOnOff(ISettingsManager settingsManager, IBusDeviceManager busDeviceManager)
        {
            this.busDeviceManager = busDeviceManager;
            Topic = settingsManager.GlobalSettings.CommandSettings.SensorOnOffTopic;
        }

        public bool Execute(CmdSensorOnOffData content)
        {
            if (content == null)
            {
                Debug.WriteLine($"Command: Sensor On/Off -> No Payload!");
                return false;
            }

            try
            {
                var sensor = busDeviceManager.GetSensorFromName(content.Name);

                if(sensor == null)
                {
                    return false;
                }

                switch(content.Action)
                {
                    case CmdSensorOnOffEnum.AddOnly:
                        Debug.WriteLine($"Command: Adding Sensor: {content.Name}.");
                        busDeviceManager.AddSensor(sensor);
                        break;
                    case CmdSensorOnOffEnum.On:
                        Debug.WriteLine($"Command: Turn Sensor: {content.Name} ON.");
                        busDeviceManager.AddSensor(sensor);
                        busDeviceManager.StartSensor(sensor);
                        break;
                    case CmdSensorOnOffEnum.Off:
                        Debug.WriteLine($"Command: Turn Sensor: {content.Name} OFF.");
                        busDeviceManager.StopSensor(sensor);
                        break;
                    case CmdSensorOnOffEnum.Delete:
                        Debug.WriteLine($"Command: Turn Sensor: {content.Name} OFF.");
                        busDeviceManager.StopSensor(sensor);
                        busDeviceManager.DeleteSensor(sensor);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Sensor On/OFF command: {ex.Message}");
                return false;
            }

            return true;
        }

        //Execute the command
        public new void Execute(string content)
        {
            base.mreExecute.WaitOne();

            CmdSensorOnOffData data = (CmdSensorOnOffData)JsonConvert.DeserializeObject(content, typeof(CmdSensorOnOffData));
            if (Execute(data))
                base.Execute(content);

            base.mreExecute.Set();
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdSensorOnOffData
    {
        public string Name { get; set; }
        public CmdSensorOnOffEnum Action { get; set; }
    }

    internal enum CmdSensorOnOffEnum
    {
        On, 
        Off, 
        AddOnly,
        Delete
    }
}