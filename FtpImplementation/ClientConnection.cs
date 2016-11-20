using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using FtpImplementation.FtpCommands;

namespace FtpImplementation
{
    public class ClientConnection
    {
        public TcpClient TcpClient { get; }
        public TcpClient DataClient { get; set; }

        private readonly StreamReader _reader;
        public StreamWriter Writer { get; }
        
        public TcpListener PassiveListener { get; set; }
        public IPEndPoint DataEndpoint { get; set; }

        public string CurrentDirectory { get { return "E:\\"; } } 

        public DataConnectionType DataConnectionType {get { return DataConnectionType.Passive; } }

        public string TransferType { get; set; }

        public ClientConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            var networkStream = TcpClient.GetStream();
            _reader = new StreamReader(networkStream);
            Writer = new StreamWriter(networkStream);
        }

        public void HandleClient(object obj)
        {
            Writer.WriteLine("220 Service Ready.");
            Writer.Flush();
            try
            {
                string line;
                while (!string.IsNullOrEmpty(line = _reader.ReadLine()))
                {
                    var commandStr = line.Split(' ');
                    var cmd = commandStr[0].ToUpperInvariant();
                    var arguments = commandStr.Length > 1 ? line.Substring(commandStr[0].Length + 1) : null;

                    if (String.IsNullOrWhiteSpace(arguments)) arguments = null;
                    var command = FtpCommandsController.Instance.GetCommand(cmd,this);
                    var response = command != null ? command.Execute(arguments) : "502 Command not implemented";
                        
                    if (TcpClient == null || !TcpClient.Connected) break;
                    Writer.WriteLine(response);
                    Writer.Flush();
                    if (response.StartsWith("221")) break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
