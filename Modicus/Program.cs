using System.Diagnostics;
using System.Threading;
using GardenLightHyperionConnector.Manager;
using Modicus.Commands.Interfaces;
using Modicus.Helpers;
using Modicus.Helpers.Interfaces;
using Modicus.Interfaces;
using Modicus.Manager;
using Modicus.MQTT.Interfaces;
using Modicus.Web;
using Modicus.Wifi.Interfaces;
using nanoFramework.DependencyInjection;

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

            ServiceProvider services = ConfigureServices();
            ModicusStartupManager modicusStartupManager = (ModicusStartupManager)services.GetRequiredService(typeof(ModicusStartupManager));
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Configure the Dependency Injection Services
        /// </summary>
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(typeof(ModicusStartupManager))
                .AddSingleton(typeof(IWebManager), typeof(WebManager))
                .AddSingleton(typeof(ISignalService), typeof(SignalService))
                .AddSingleton(typeof(ISettingsManager), typeof(SettingsManager))
                .AddSingleton(typeof(IWiFiManager), typeof(WiFiManager))
                .AddSingleton(typeof(ITokenManager), typeof(TokenManager))
                .AddSingleton(typeof(IMqttManager), typeof(MqttManager))
                .AddSingleton(typeof(ICommandManager), typeof(CommandManager))
                .AddSingleton(typeof(ModicusWebpageAPI))
                .AddSingleton(typeof(ModicusWebpages))
                .BuildServiceProvider();
        }
    }
}