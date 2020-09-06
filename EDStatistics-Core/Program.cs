using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ComputeSharp;
using Processing;

namespace EDStatistics_Core
{
    class Program
    {
        static void Main()
        {
            // Loads coordinates from a custom-format bin file into memory.

            Console.WriteLine("Loading coordinates into memory...");
            var systemCount = (new FileInfo("systemsNew.bin")).Length / Converter.systemByteSize;
            var coordinates = new double[systemCount * 3];
            var sysI = 0;

            var systemBufferSizeTarget = 50000;
            using (var fileStream = new FileStream("systemsNew.bin", FileMode.Open, FileAccess.Read))
            {
                while (systemCount > 0)
                {
                    var systemsToRead = systemCount < systemBufferSizeTarget ? systemCount : systemBufferSizeTarget;
                    systemCount -= systemsToRead;

                    var buffer = new byte[systemsToRead * Converter.systemByteSize];
                    fileStream.Read(buffer, 0, (int)systemsToRead * Converter.systemByteSize);

                    for (var i = 0; i < systemsToRead; i++)
                    {
                        var index = (i * Converter.systemByteSize) + sizeof(long);
                        var x = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        var y = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        var z = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        coordinates[sysI] = x;
                        coordinates[sysI + 1] = y;
                        coordinates[sysI + 2] = z;

                        sysI += 3;
                    }
                }
            }
            var coordinatesBuffer = Gpu.Default.AllocateReadOnlyBuffer(coordinates);
            Console.WriteLine("Done loading coordinates. " + (sysI / 3) + " systems loaded.");

            // Creating/handling the frames folder.
            if (Directory.Exists("frames")) {
                foreach (var f in Directory.GetFiles("frames")) { File.Delete(f); } }
            else { Directory.CreateDirectory("frames");}
            Thread.Sleep(500);

            // This is the total range the coordinates can be in, so we can a good idea of a default viewport.
            var galMin = new Coordinates(-42213.81f, -29359.81f, -23405f);
            var galMax = new Coordinates(40503.81f, 39518.34f, 65630.16f);

            // For animating the frames, I lerp between viewpoints for each frame. These are the 2 keys that the lerping follows.
            var from = new Viewport(galMin.z, galMax.z, galMax.x, galMin.x);
            var to = new Viewport(10, -10, 10, -10);

            /// This just determines how many frames to render.
            var timeToTake = 30; // seconds
            var frameRate = 60;

            var progress = 0;

            var watch = new Stopwatch();
            watch.Start();
            var previousMaxDensity = -1d;
            for (var i = 0; i < frameRate * timeToTake; i++)
            {
                var t = i / (double)(frameRate * timeToTake);
                t = Smoothing(t);
                var image = GalaxyRenderer.Render(
                    Viewport.Lerp(from, to, t), 1000, 1000,
                    GalaxyRenderer.DefaultColorMapping,
                    GalaxyRenderer.DefaultSmoothing,
                    i == 0 ? (double?)null : previousMaxDensity, out previousMaxDensity,
                    ref coordinatesBuffer);
                //image.Save(@"frames\" + i.ToString("000000") + ".png");
                //image.Dispose();
                progress++;

                #region Logging progress per frame
                Console.Clear();
                Console.WriteLine(progress + " / " + (frameRate * timeToTake) + "  -  " + ((progress * 100) / (double)(frameRate * timeToTake)).ToString("0.00") + "%");

                var secondsPerFrame = (double)watch.Elapsed.TotalSeconds / progress;
                var framesRemaining = (frameRate * timeToTake) - progress;
                var timeRemaining = new TimeSpan((long)(10000000 * (framesRemaining * secondsPerFrame)));
                Console.WriteLine("Estimated time remaining: " +
                    timeRemaining.Hours.ToString("00") + ":" +
                    timeRemaining.Minutes.ToString("00") + ":" +
                    timeRemaining.Seconds.ToString("00"));
                Console.WriteLine("Average time per frame: " + secondsPerFrame.ToString("0.00") + " seconds.");
                #endregion
            }
        }

        static double Sigmoid(double t) => 1f / (1f + Math.Pow(2.71828d, -t));
        static double Smoothing(double t) => Sigmoid((10f * t) - 5f);
    }
}