using System;
using System.Net;
using System.Net.Sockets;

namespace FtpImplementation.FtpCommands
{
    public class PassiveCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }
        public string Execute(string parameter)
        {
            var localIp = ((IPEndPoint)ClientConnection.TcpClient.Client.LocalEndPoint).Address;

            ClientConnection.PassiveListener = new TcpListener(localIp, 0);
            ClientConnection.PassiveListener.Start();

            var passiveListenerEndpoint = (IPEndPoint)ClientConnection.PassiveListener.LocalEndpoint;

            var address = passiveListenerEndpoint.Address.GetAddressBytes();
            var port = (short)passiveListenerEndpoint.Port;

            var portArray = BitConverter.GetBytes(port);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArray);

            return $"227 Entering Passive Mode ({address[0]},{address[1]},{address[2]},{address[3]},{portArray[0]},{portArray[1]})";
        }
    }
}
