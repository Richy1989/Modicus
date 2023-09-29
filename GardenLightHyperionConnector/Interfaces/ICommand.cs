using GardenLightHyperionConnector.Commands;

namespace Modicus.Interfaces
{
    internal interface ICommand
    {
        event CommandRaisedHandler CommandRaisedEvent;
        public void Execute(string content);
        string Topic { get; set; }
    }
}
