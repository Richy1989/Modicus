using System;
using System.Collections.Generic;
using System.Text;

namespace Modicus.Interfaces
{
    internal interface ICommand
    {
        public void Execute(string content);
    }
}
