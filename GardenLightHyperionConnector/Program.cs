using System.Diagnostics;
using System.Threading;
using Modicus.Manager;

namespace Modicus
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from Modicus!");

            ModicusStartupManager envLightManager = new();

            //controller.ClosePin(Gpio.IO02);
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
