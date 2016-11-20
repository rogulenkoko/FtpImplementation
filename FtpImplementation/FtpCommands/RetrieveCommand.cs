using System;
using System.IO;
using System.Net.Sockets;

namespace FtpImplementation.FtpCommands
{
    public class RetrieveCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }
        public string Execute(string parameter)
        {
            var pathname = NormalizeFilename(parameter);
            if (File.Exists(pathname))
            {
                if (ClientConnection.DataConnectionType == DataConnectionType.Active)
                {
                    ClientConnection.DataClient = new TcpClient();
                    ClientConnection.DataClient.BeginConnect(ClientConnection.DataEndpoint.Address, ClientConnection.DataEndpoint.Port, DoRetrieve, pathname);
                }
                else
                {
                    ClientConnection.PassiveListener.BeginAcceptTcpClient(DoRetrieve, pathname);
                }

                return $"150 Opening {ClientConnection.DataConnectionType} mode data transfer for RETR";
            }
            

            return "550 File Not Found";
        }

        private void DoRetrieve(IAsyncResult result)
        {
            if (ClientConnection.DataConnectionType == DataConnectionType.Active)
            {
                ClientConnection.DataClient.EndConnect(result);
            }
            else
            {
                ClientConnection.DataClient = ClientConnection.PassiveListener.EndAcceptTcpClient(result);
            }

            var pathname = (string)result.AsyncState;

            using (var dataStream = ClientConnection.DataClient.GetStream())
            {
                using (var fileStream = new FileStream(pathname, FileMode.Open, FileAccess.Read))
                {
                    CopyStreamHelper.CopyStream(fileStream, dataStream, ClientConnection.TransferType);

                    ClientConnection.DataClient.Close();
                    ClientConnection.DataClient = null;
                    ClientConnection.Writer.WriteLine("226 Closing data connection, file transfer successful");
                    ClientConnection.Writer.Flush();
                }
            }
        }

        private string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == "/")
            {
                return ClientConnection.CurrentDirectory;
            }
            else if (path.StartsWith("/"))
            {
                path = new FileInfo(Path.Combine(ClientConnection.CurrentDirectory, path.Substring(1))).FullName;
            }
            else
            {
                path = new FileInfo(Path.Combine(ClientConnection.CurrentDirectory, path)).FullName;
            }

            return path;
        }
    }
}
