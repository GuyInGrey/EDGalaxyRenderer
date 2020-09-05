using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace EDStatistics
{
    public static class Converter
    {
        public static void ConvertExplorationData(string from, string to)
        {
            var count = 0;
            if (File.Exists(to)) { File.Delete(to); }
            using (File.Create(to)) { }

            var bufferSize = 50000 * 70;

            var buffer = new byte[bufferSize];
            var bufferIndex = 0;

            const int BufferSize = 4096;
            using (var bufferWriteStream = new FileStream(to, FileMode.Append))
            using (var fileStream = File.OpenRead(from))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    count++;
                    if (count % 500000 == 0) { Console.WriteLine("Loading star systems, please wait...  " + count + "  (" + ((count / 54000000f) * 100f).ToString("0.00") + "%)"); }

                    line = line.Trim();
                    if (line.Length < 3) { continue; }
                    if (line.Substring(line.Length - 1, 1).Equals(","))
                    {
                        line = line.Substring(0, line.Length - 1);
                    }
                    var s = JsonConvert.DeserializeObject<StarSystem>(line);
                    if (s is null) { continue; }

                    Array.Copy(BitConverter.GetBytes(s.id64), 0, buffer, bufferIndex, sizeof(long)); bufferIndex += sizeof(long);
                    Array.Copy(BitConverter.GetBytes(s.coords.x), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                    Array.Copy(BitConverter.GetBytes(s.coords.y), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                    Array.Copy(BitConverter.GetBytes(s.coords.z), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                    Array.Copy(Encoding.ASCII.GetBytes(s.name.PadRight(50, ' ')), 0, buffer, bufferIndex, 50); bufferIndex += 50;

                    if (bufferIndex >= buffer.Length)
                    {
                        bufferWriteStream.Write(buffer, 0, buffer.Length);
                        buffer = new byte[bufferSize];
                        bufferIndex = 0;
                    }
                }

                bufferWriteStream.Write(buffer, 0, bufferIndex);
            }
        }
    }
}
