using Modicus.EventArgs;
using Modicus.Interfaces;

namespace Modicus.Commands
{
    // Declare the delegate (if using non-generic pattern).
    internal delegate void CommandRaisedHandler(object sender, CommandRaisedEventArgs e);

    internal abstract class BaseCommand : ICommand
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