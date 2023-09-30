using Modicus.Commands.Interfaces;

namespace Modicus.EventArgs
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