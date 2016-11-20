using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FtpImplementation
{
    public class FtpServer
    {
        private TcpListener _tcpListener;

        public void Start()
        {
            _tcpListener = new TcpListener(IPAddress.Any,21);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(ListenToTcpClient, _tcpListener);
        }


        private void ListenToTcpClient(IAsyncResult result)
        {
            _tcpListener.BeginAcceptTcpClient(ListenToTcpClient, _tcpListener);
            var client = _tcpListener.EndAcceptTcpClient(result);
           
            var clientConnection = new ClientConnection(client);
            ThreadPool.QueueUserWorkItem(clientConnection.HandleClient, client);
        }
    }
}
