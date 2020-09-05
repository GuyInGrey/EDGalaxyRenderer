using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Processing;

namespace EDStatistics
{
    class Program
    {
        static void Main()
        {
            if (Directory.Exists("frames"))
            {
                foreach (var f in Directory.GetFiles("frames"))
                {
                    File.Delete(f);
                }
            }
            else
            {
                Directory.CreateDirectory("frames");
            }
            Thread.Sleep(500);

            var galMin = new Coordinates(-42213.81f, -29359.81f, -23405f);
            var galMax = new Coordinates(40503.81f, 39518.34f, 65630.16f);

            var from = new Viewport()
            {
                Top = galMax.z,
                Bottom = galMin.z,
                Right = galMax.x,
                Left = galMin.x,
            };

            var to = new Viewport()
            {
                Top = 100,
                Bottom = -100,
                Right = 100,
                Left = -100,
            };

            var timeToTake = 60; // seconds
            var frameRate = 60;

            var progress = 0;

            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, frameRate * timeToTake, (i) =>
            {
                var t = i / (float)(frameRate * timeToTake);
                t = Smoothing(t);
                var image = GalaxyRenderer.Render(Viewport.Lerp(from, to, t), 1000, 1000, GalaxyRenderer.DefaultColorMapping);
                image.Save(@"frames\" + i.ToString("000000") + ".png");
                image.Dispose();
                progress++;

                Console.Clear();
                Console.WriteLine(progress + " / " + (frameRate * timeToTake) + "  -  " + ((progress * 100) / (float)(frameRate * timeToTake)).ToString("0.00") + "%");

                var secondsPerFrame = (float)watch.Elapsed.TotalSeconds / progress;
                var framesRemaining = (frameRate * timeToTake) - progress;
                var timeRemaining = new TimeSpan((long)(10000000 * (framesRemaining * secondsPerFrame)));
                Console.WriteLine("Estimated time remaining: " +
                    timeRemaining.Hours.ToString("00") + ":" +
                    timeRemaining.Minutes.ToString("00") + ":" +
                    timeRemaining.Seconds.ToString("00"));
                Console.WriteLine("Average time per frame: " + secondsPerFrame.ToString("0.00") + " seconds.");
            });
        }

        static float Sigmoid(float t) => 1f / (1f + PMath.Pow(2.71828f, -t));
        static float Smoothing(float t) => Sigmoid((10f * t) - 5f);
    }
}