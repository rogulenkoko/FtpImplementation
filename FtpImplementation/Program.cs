using System;

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
