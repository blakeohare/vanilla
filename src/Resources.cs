using System;
using System.Collections.Generic;
using System.Linq;

namespace Vanilla
{
    internal static class Resources
    {
        private static readonly byte[] BUFFER = new byte[1024];
        public static byte[] GetResourceBytes(string path)
        {
            string resPath = "Vanilla." + path.Replace('\\', '.').Replace('/', '.');
            string[] lol = typeof(Resources).Assembly.GetManifestResourceNames();
            System.IO.Stream stream = typeof(Resources).Assembly.GetManifestResourceStream(resPath);
            List<byte> bytes = new List<byte>();
            int bytesRead = 1;
            do
            {
                bytesRead = stream.Read(BUFFER, 0, BUFFER.Length);
                if (bytesRead == BUFFER.Length)
                {
                    bytes.AddRange(BUFFER);
                }
                else
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        bytes.Add(BUFFER[i]);
                    }
                }
            } while (bytesRead > 0);

            return bytes.ToArray();
        }

        public static string GetResourceText(string path)
        {
            byte[] bytes = GetResourceBytes(path);
            if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                bytes = bytes.Skip(3).ToArray();
            }
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
