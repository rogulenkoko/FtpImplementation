using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FtpImplementation
{
    public class ClientConnection
    {
        private TcpClient _tcpClient;
        private TcpClient _dataClient;
        private NetworkStream _networkStream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private StreamReader _dataReader;
        private StreamWriter _dataWriter;

        private TcpListener _passiveListener;
        private IPEndPoint _dataEndpoint;

        private string _currentDirectory;

        private DataConnectionType _dataConnectionType = DataConnectionType.Passive;

        private string _transferType;
        private string _userName;

        public ClientConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream);
        }

        public void HandleClient(object obj)
        {
            _writer.WriteLine("220 Service Ready.");
            _writer.Flush();

            string line;

            try
            {
                while (!string.IsNullOrEmpty(line = _reader.ReadLine()))
                {
                    string response = null;

                    string[] command = line.Split(' ');

                    string cmd = command[0].ToUpperInvariant();
                    string arguments = command.Length > 1 ? line.Substring(command[0].Length + 1) : null;

                    if (string.IsNullOrWhiteSpace(arguments))
                        arguments = null;

                    if (response == null)
                    {
                        switch (cmd)
                        {
                            case "USER":
                                response = User(arguments);
                                break;
                            case "PASS":
                                response = "230 User logged in";
                                break;
                            case "CWD":
                                response = ChangeWorkingDirectory(arguments);
                                break;
                            case "CDUP":
                                response = ChangeWorkingDirectory("E:\\");
                                break;
                            case "PWD":
                                response = "257 \"/\" is current directory.";
                                break;
                            case "PORT":
                                response = Port(arguments);
                                break;
                            case "PASV":
                                response = Passive();
                                break;
                            case "LIST":
                                response = List(arguments);
                                break;
                            case "RETR":
                                response = Retrieve(arguments);
                                break;
                            case "QUIT":
                                response = "221 Service closing control connection";
                                break;
                            case "TYPE":
                                string[] splitArgs = arguments.Split(' ');
                                response = Type(splitArgs[0], splitArgs.Length > 1 ? splitArgs[1] : null);
                                break;

                            default:
                                response = "502 Command not implemented";
                                break;
                        }
                    }

                    if (_tcpClient == null || !_tcpClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        _writer.WriteLine(response);
                        _writer.Flush();

                        if (response.StartsWith("221"))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private string Port(string hostPort)
        {

            string[] ipAndPort = hostPort.Split(',');

            byte[] ipAddress = new byte[4];
            byte[] port = new byte[2];

            for (int i = 0; i < 4; i++)
            {
                ipAddress[i] = Convert.ToByte(ipAndPort[i]);
            }

            for (int i = 4; i < 6; i++)
            {
                port[i - 4] = Convert.ToByte(ipAndPort[i]);
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(port);

            _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));

            return "200 Data Connection Established";
        }

        private string Passive()
        { 
            var localIp = ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Address;

            _passiveListener = new TcpListener(localIp, 0);
            _passiveListener.Start();

            var passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;

            var address = passiveListenerEndpoint.Address.GetAddressBytes();
            var port = (short)passiveListenerEndpoint.Port;

            var portArray = BitConverter.GetBytes(port);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(portArray);

            return $"227 Entering Passive Mode ({address[0]},{address[1]},{address[2]},{address[3]},{portArray[0]},{portArray[1]})";
        }

        private string Type(string typeCode, string formatControl)
        {
            string response = "";

            switch (typeCode)
            {
                case "A":
                    break;
                case "I":
                    _transferType = typeCode;
                    response = "200 OK";
                    break;
                case "E":
                case "L":
                default:
                    response = "504 Command not implemented for that parameter.";
                    break;
            }

            if (formatControl != null)
            {
                switch (formatControl)
                {
                    case "N":
                        response = "200 OK";
                        break;
                    case "T":
                    case "C":
                    default:
                        response = "504 Command not implemented for that parameter.";
                        break;
                }
            }

            return response;
        }

        private string List(string pathName)
        {
            if (pathName == null) pathName = string.Empty;
            if(_currentDirectory==null) _currentDirectory = "E:\\";

            pathName = new DirectoryInfo(Path.Combine(_currentDirectory, pathName)).FullName;

            if (IsPathValid(pathName))
            {
                if (_dataConnectionType == DataConnectionType.Active)
                {
                    _dataClient = new TcpClient();
                    _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoList, pathName);
                }
                else
                {
                    _passiveListener.BeginAcceptTcpClient(DoList, pathName);
                }

                return string.Format("150 Opening {0} mode data transfer for LIST", _dataConnectionType);
            }
            return "450 Requested file action not taken";
        }

        private void DoList(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient.EndConnect(result);
            }
            else
            {
                _dataClient = _passiveListener.EndAcceptTcpClient(result);
            }

            var pathname = (string)result.AsyncState;
            using (var dataStream = _dataClient.GetStream())
            {
                _dataReader = new StreamReader(dataStream, Encoding.ASCII);
                _dataWriter = new StreamWriter(dataStream, Encoding.ASCII);

                IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

                foreach (string dir in directories)
                {
                    var d = new DirectoryInfo(dir);

                    var date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180)
                        ? d.LastWriteTime.ToString("MMM dd  yyyy")
                        : d.LastWriteTime.ToString("MMM dd HH:mm");

                    var line = $"drwxr-xr-x    2 2003     2003     {"4096",8} {date} {d.Name}";

                    _dataWriter.WriteLine(line);
                    _dataWriter.Flush();
                }

                IEnumerable<string> files = Directory.EnumerateFiles(pathname);

                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);

                    string date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                        f.LastWriteTime.ToString("MMM dd  yyyy") :
                        f.LastWriteTime.ToString("MMM dd HH:mm");

                    string line = string.Format("-rw-r--r--       2003     {0} ", f.Name);

                    _dataWriter.WriteLine(line);
                    _dataWriter.Flush();
                }
                
                _dataClient.Close();
                _dataClient = null;

                _writer.WriteLine("226 Transfer complete");
                _writer.Flush();


            }
            
        }

        private string Retrieve(string pathname)
        {
            pathname = NormalizeFilename(pathname);

            if (IsPathValid(pathname))
            {
                if (File.Exists(pathname))
                {
                    if (_dataConnectionType == DataConnectionType.Active)
                    {
                        _dataClient = new TcpClient();
                        _dataClient.BeginConnect(_dataEndpoint.Address, _dataEndpoint.Port, DoRetrieve, pathname);
                    }
                    else
                    {
                        _passiveListener.BeginAcceptTcpClient(DoRetrieve, pathname);
                    }

                    return string.Format("150 Opening {0} mode data transfer for RETR", _dataConnectionType);
                }
            }

            return "550 File Not Found";
        }

        private string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == "/")
            {
                return _currentDirectory;
            }
            else if (path.StartsWith("/"))
            {
                path = new FileInfo(Path.Combine(_currentDirectory, path.Substring(1))).FullName;
            }
            else
            {
                path = new FileInfo(Path.Combine(_currentDirectory, path)).FullName;
            }

            return IsPathValid(path) ? path : null;
        }

        private void DoRetrieve(IAsyncResult result)
        {
            if (_dataConnectionType == DataConnectionType.Active)
            {
                _dataClient.EndConnect(result);
            }
            else
            {
                _dataClient = _passiveListener.EndAcceptTcpClient(result);
            }

            string pathname = (string)result.AsyncState;

            using (var dataStream = _dataClient.GetStream())
            {
                using (var fileStream = new FileStream(pathname,FileMode.Open,FileAccess.Read))
                {
                    CopyStreamHelper.CopyStream(fileStream, dataStream,_transferType);

                    _dataClient.Close();
                    _dataClient = null;
                    _writer.WriteLine("226 Closing data connection, file transfer successful");
                    _writer.Flush();
                }
            }
        }

        private bool IsPathValid(string path)
        {
            return true;
        }

        private string User(string username)
        {
            _userName = username;

            return "331 Username ok, need password";
        }

       

        private string ChangeWorkingDirectory(string pathname)
        {
            _currentDirectory = "E:\\";
            return "250 Changed to new directory";
        }
    }
}
