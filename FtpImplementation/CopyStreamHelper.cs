
using System.IO;
using System.Text;

namespace FtpImplementation
{
    public static class CopyStreamHelper
    {
        private static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        private static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
        {
            var buffer = new char[bufferSize];
            var total = 0;

            using (StreamReader rdr = new StreamReader(input))
            {
                using (StreamWriter wtr = new StreamWriter(output, Encoding.ASCII))
                {
                    int count;
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                    }
                }
            }

            return total;
        }

        public static long CopyStream(Stream input, Stream output, string transferType)
        {
            if (transferType == "I") return CopyStream(input, output, 4096);
            return CopyStreamAscii(input, output, 4096);
        }
    }
}
