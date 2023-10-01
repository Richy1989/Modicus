﻿using System;
using System.Diagnostics;
using System.Threading;
using Modicus.Interfaces;
using Modicus.MQTT.Interfaces;
using nanoFramework.Json;
using nanoFramework.Runtime.Native;

namespace Modicus.Commands
{
    //With this command the ;QTT Service can be turned on and off
    internal class CmdSystemReboot : BaseCommand
    {
        public CmdSystemReboot(ISettingsManager settingsManager)
        {
            Topic = settingsManager.GlobalSettings.CommandSettings.RebootControllerTopic;
        }

        public void Execute(CmdRebootControllerData content)
        {
            base.mreExecute.WaitOne();

            int delay = content != null ? content.Delay : 0;
            var commandTime = DateTime.UtcNow;

            while((DateTime.UtcNow - commandTime).TotalSeconds < delay)
            {
                Thread.Sleep(1000);
            }

            Power.RebootDevice();

            base.mreExecute.Set();
        }

        //Execute the command
        public new void Execute(string content)
        {
            CmdRebootControllerData data = (CmdRebootControllerData)JsonConvert.DeserializeObject(content, typeof(CmdRebootControllerData));
            Execute(data);
            base.Execute(content);
        }
    }

    //This class contraints the content of the message which will be sent via JSON
    internal class CmdRebootControllerData
    {
        public int Delay { get; set; }
    }
}