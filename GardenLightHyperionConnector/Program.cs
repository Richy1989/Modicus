using System.Diagnostics;
using System.Threading;
using Modicus.Manager;

namespace Modicus
{
    public class Program
    {
        /// <summary>
        /// Main class for Modicus, this initializes the main manager
        /// </summary>
        public static void Main()
        {
            Debug.WriteLine("Hello from Modicus!");

            ModicusStartupManager modicusStartupManager = new();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
