using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FtpImplementation.FtpCommands
{
    public class ListCommand : ICommand
    {
        public ClientConnection ClientConnection { get; set; }

        public string Execute(string parameter)
        {
            var pathName = parameter ?? string.Empty;

            pathName = new DirectoryInfo(Path.Combine(ClientConnection.CurrentDirectory, pathName)).FullName;
            
            if (ClientConnection.DataConnectionType == DataConnectionType.Active)
            {
                ClientConnection.DataClient = new TcpClient();
                ClientConnection.DataClient.BeginConnect(
                    ClientConnection.DataEndpoint.Address, ClientConnection.DataEndpoint.Port, DoList, pathName);
            }
            else
            {
                ClientConnection.PassiveListener.BeginAcceptTcpClient(DoList, pathName);
            }

            return $"150 Opening {ClientConnection.DataConnectionType} mode data transfer for LIST";
        }

        private void DoList(IAsyncResult result)
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
                var dataWriter = new StreamWriter(dataStream, Encoding.ASCII);

                var directories = Directory.EnumerateDirectories(pathname);

                foreach (string dir in directories)
                {
                    var d = new DirectoryInfo(dir);

                    var date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180)
                        ? d.LastWriteTime.ToString("MMM dd  yyyy")
                        : d.LastWriteTime.ToString("MMM dd HH:mm");

                    var line = $"drwxr-xr-x    2 2003     2003     {"4096",8} {date} {d.Name}";

                    dataWriter.WriteLine(line);
                    dataWriter.Flush();
                }

                var files = Directory.EnumerateFiles(pathname);

                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);
                    string line = $"-rw-r--r--       2003     {f.Name} ";

                    dataWriter.WriteLine(line);
                    dataWriter.Flush();
                }

                ClientConnection.DataClient.Close();
                ClientConnection.DataClient = null;

                ClientConnection.Writer.WriteLine("226 Transfer complete");
                ClientConnection.Writer.Flush();
            }
        }
    }
}
