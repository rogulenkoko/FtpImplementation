using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FtpImplementation
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new FtpServer();
            server.Start();
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }
    }
}
