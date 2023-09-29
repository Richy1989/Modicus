using System;
using System.Collections.Generic;
using System.Text;
using Modicus.Interfaces;

namespace GardenLightHyperionConnector.EventArgs
{
    internal class CommandRaisedEventArgs
    {
        public ICommand Command { get; } // readonly

        public CommandRaisedEventArgs(ICommand command)
        {
            Command = command;
        }
    }
}
