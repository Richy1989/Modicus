using System;
using System.Collections.Generic;
using System.Text;
using Modicus.Interfaces;

namespace GardenLightHyperionConnector.Interfaces
{
    internal interface ICommandCapable
    {
        void RegisterCommand(ICommand command);
    }
}
