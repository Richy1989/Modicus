namespace Modicus.Commands.Interfaces
{
    internal interface ICommandCapable
    {
        void RegisterCommand(ICommand command);
    }
}