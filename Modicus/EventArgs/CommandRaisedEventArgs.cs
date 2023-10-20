using Modicus.Commands.Interfaces;

namespace Modicus.EventArgs
{
    internal class CommandRaisedEventArgs
    {
        public ICommand Command { get; } // readonly

        /// <summary>Initializes a new instance of the <see cref="CommandRaisedEventArgs"/> class.</summary>
        /// <param name="command">The command.</param>
        public CommandRaisedEventArgs(ICommand command)
        {
            Command = command;
        }
    }
}