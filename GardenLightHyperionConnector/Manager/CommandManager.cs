﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GardenLightHyperionConnector.Commands;
using GardenLightHyperionConnector.Interfaces;
using Modicus.Settings;

namespace GardenLightHyperionConnector.Manager
{
    internal class CommandManager
    {
        public CmdMeasurementInterval CmdMeasurementInterval { get; }
        public IDictionary CommandCapableManagers { get; }


        public CommandManager(GlobalSettings globalSettings)
        {
            CmdMeasurementInterval = new CmdMeasurementInterval(globalSettings);
            CommandCapableManagers = new Hashtable();
        }
        public void AddCommandCapableManager(Type type, ICommandCapable commandCapable)
        {
            CommandCapableManagers.Add(type, commandCapable);
        }

        public void Init()
        {   
            
        }

        public void SetMqttCommands()
        {
            CmdMeasurementInterval.Topic = "measurementinterval";
            
            foreach(Type item in CommandCapableManagers.Keys)
            {
                ((ICommandCapable)CommandCapableManagers[item]).RegisterCommand(CmdMeasurementInterval);
            }
        }
    }
}