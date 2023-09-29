using System;
using System.Collections.Generic;
using System.Text;
using GardenLightHyperionConnector.EventArgs;
using Modicus.Commands;
using Modicus.Interfaces;

namespace GardenLightHyperionConnector.Commands
{
    // Declare the delegate (if using non-generic pattern).
    internal delegate void CommandRaisedHandler(object sender, CommandRaisedEventArgs e);

    abstract class BaseCommand : ICommand
    {
        public string Topic { get; set; }

        public event CommandRaisedHandler CommandRaisedEvent;

        public BaseCommand(string topic)
        {
            this.Topic = topic;
        }

        public void Execute(string content)
        {
            CommandRaisedEvent?.Invoke(this, new CommandRaisedEventArgs(this));
        }
    }
}
