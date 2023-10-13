using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using nanoFramework.Logging.Stream;
using nanoFramework.Logging;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Modicus.Logger.Interface;

namespace Modicus.Logger
{
    internal class TcpLogger : ITcpLogger
    {
        public TcpLogger()
        {
            TcpClient client = new TcpClient();
            client.Connect("192.168.0.232", 878);
            NetworkStream stream = client.GetStream();

            //LogDispatcher.LoggerFactory = new StreamLoggerFactory(stream);

            StreamLogger _logger = new StreamLogger(stream, "name");
            _logger.MinLogLevel = LogLevel.Trace;
            _logger.LogTrace("This is a trace");

        }
    }
}
