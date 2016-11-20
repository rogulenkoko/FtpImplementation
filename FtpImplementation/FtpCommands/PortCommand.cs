using System;
using System.Net;

namespace FtpImplementation.FtpCommands
{
    public class PortCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }
        public string Execute(string parameter)
        {
            var ipAndPort = parameter.Split(',');

            var ipAddress = new byte[4];
            var port = new byte[2];

            for (var i = 0; i < 4; i++)
            {
                ipAddress[i] = Convert.ToByte(ipAndPort[i]);
            }

            for (var i = 4; i < 6; i++)
            {
                port[i - 4] = Convert.ToByte(ipAndPort[i]);
            }

            if (BitConverter.IsLittleEndian) Array.Reverse(port);
            ClientConnection.DataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

            return "200 Data Connection Established";
        }
    }
}
