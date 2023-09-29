using System;
using System.Collections.Generic;
using System.Text;
using Modicus.Interfaces;

namespace Modicus.Interfaces
{
    internal interface ICommandCapable
    {
        void RegisterCommand(ICommand command);
    }
}
