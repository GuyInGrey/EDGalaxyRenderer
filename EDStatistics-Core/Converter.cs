using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace EDStatistics_Core
{
    public static class Converter
    {
        public static int systemByteSize => sizeof(long) + sizeof(double) + (3 * sizeof(float)) + 50;

        public static void ConvertExplorationData(string from, string to)
        {
            try
            {
                Console.WriteLine("Beggining conversion: " + from + "  ->  " + to);
                var count = 0;
                if (File.Exists(to)) { Console.WriteLine("Deleted old bin file."); File.Delete(to); }
                using (File.Create(to)) { }

                var bufferSize = 50000 * systemByteSize;
                Console.WriteLine("bufferSize: " + bufferSize + "\n" + "systemByteSize: " + systemByteSize);

                var buffer = new byte[bufferSize];
                var bufferIndex = 0;

                const int BufferSize = 4096;
                Console.WriteLine("Attemping to open bin stream...");
                using var bufferWriteStream = new FileStream(to, FileMode.Append);
                Console.WriteLine("Attemping to open json filestream...");
                using var fileStream = File.OpenRead(from);
                Console.WriteLine("Attemping to open json streamreader...");
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize);
                Console.WriteLine("Entered file reading...");
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    try
                    {
                        //Console.WriteLine("Read line. count: " + count);
                        if (count == 0) { count++; continue; }
                        count++;
                        if (count % 500000 == 0) { Console.WriteLine("Converting star system format...  " + count + "  (" + ((count / 54000000d) * 100d).ToString("0.00") + "%)"); }

                        line = line.Trim();
                        if (line.Length < 3) { continue; }
                        if (line.Substring(line.Length - 1, 1).Equals(","))
                        {
                            line = line.Substring(0, line.Length - 1);
                        }
                        var s = JsonConvert.DeserializeObject<StarSystem>(line);
                        if (s is null) { continue; }

                        Array.Copy(BitConverter.GetBytes(s.id64), 0, buffer, bufferIndex, sizeof(long)); bufferIndex += sizeof(long);
                        Array.Copy(BitConverter.GetBytes(s.dateTime.ConvertToUnixTimestamp()), 0, buffer, bufferIndex, sizeof(double)); bufferIndex += sizeof(double);
                        Array.Copy(BitConverter.GetBytes(s.coords.x), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                        Array.Copy(BitConverter.GetBytes(s.coords.y), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                        Array.Copy(BitConverter.GetBytes(s.coords.z), 0, buffer, bufferIndex, sizeof(float)); bufferIndex += sizeof(float);
                        Array.Copy(Encoding.ASCII.GetBytes(s.name.PadRight(50, ' ')), 0, buffer, bufferIndex, 50); bufferIndex += 50;

                        if (bufferIndex >= buffer.Length)
                        {
                            bufferWriteStream.Write(buffer, 0, buffer.Length);
                            buffer = new byte[bufferSize];
                            bufferIndex = 0;
                            Console.WriteLine("Dumped buffer.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error (A)!\n" + e.Message + "\n" + e.StackTrace);
                    }
                }

                bufferWriteStream.Write(buffer, 0, bufferIndex);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error (B)!\n" + e.Message + "\n" + e.StackTrace);
            }
        }

        public static float[] GetCoordinates(string binPath)
        {
            // Loads coordinates from a custom-format bin file into memory.
            Console.WriteLine("Loading coordinates into memory...");
            var systemCount = (new FileInfo(binPath)).Length / systemByteSize;
            var coordinates = new float[systemCount * 3];
            var sysI = 0;

            var systemBufferSizeTarget = 50000;
            using (var fileStream = new FileStream(binPath, FileMode.Open, FileAccess.Read))
            {
                while (systemCount > 0)
                {
                    var systemsToRead = systemCount < systemBufferSizeTarget ? systemCount : systemBufferSizeTarget;
                    systemCount -= systemsToRead;

                    var buffer = new byte[systemsToRead * systemByteSize];
                    fileStream.Read(buffer, 0, (int)systemsToRead * systemByteSize);

                    for (var i = 0; i < systemsToRead; i++)
                    {
                        var index = (i * systemByteSize) + (2 * sizeof(long));
                        var x = BitConverter.ToSingle(buffer, index); index += sizeof(float);
                        var y = BitConverter.ToSingle(buffer, index); index += sizeof(float);
                        var z = BitConverter.ToSingle(buffer, index); index += sizeof(float);
                        coordinates[sysI] = x;
                        coordinates[sysI + 1] = y;
                        coordinates[sysI + 2] = z;

                        sysI += 3;
                    }
                }
            }
            Console.WriteLine("Done loading coordinates. " + (sysI / 3) + " systems loaded.");

            return coordinates;
        }
    }
}
