using System;
using System.Net;
using nanoFramework.DependencyInjection;
using nanoFramework.WebServer;

namespace Modicus.Manager
{
    internal class WebServerDI : WebServer
    {
        private readonly IServiceProvider _serviceProvider;

        public WebServerDI(int port, HttpProtocol protocol, Type[] controllers, IServiceProvider serviceProvider) : base(port, protocol, controllers)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
        {
            route.Callback.Invoke(ActivatorUtilities.CreateInstance(_serviceProvider, route.Callback.DeclaringType), new object[] { new WebServerEventArgs(context) });
        }
    }
}