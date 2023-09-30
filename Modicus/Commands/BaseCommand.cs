using System.Threading;
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

        //Make sure only one thread at the time can enter the execute function
        internal readonly ManualResetEvent mreExecute = new(true);

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