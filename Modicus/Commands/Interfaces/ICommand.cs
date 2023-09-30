using Modicus.Commands;

namespace Modicus.Commands.Interfaces
{
    internal interface ICommand
    {
        event CommandRaisedHandler CommandRaisedEvent;

        public void Execute(string content);

        string Topic { get; set; }
    }
}