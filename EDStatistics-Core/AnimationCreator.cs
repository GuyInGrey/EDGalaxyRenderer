using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ComputeSharp;
using Processing;

namespace EDStatistics_Core
{
    public static class AnimationCreator
    {
        public static void CreateAnimation(Viewport from, Viewport to, ref ReadOnlyBuffer<float> coordinates, int imageWidth, int imageHeight)
        {
            // Creating/handling the frames folder.
            if (Directory.Exists("frames"))
            {
                foreach (var f in Directory.GetFiles("frames")) { File.Delete(f); }
            }
            else { Directory.CreateDirectory("frames"); }
            Thread.Sleep(500);

            var previousMaxDensity = -1f;

            /// This just determines how many frames to render.
            var timeToTake = 1; // seconds
            var frameRate = 1000;

            var progress = 0;

            var watch = new Stopwatch();
            watch.Start();
            for (var i = 0; i < frameRate * timeToTake; i++)
            {
                var t = i / (float)(frameRate * timeToTake);
                t = Smoothing(t);
                var image = new PSprite(imageWidth, imageHeight);
                image.Art.SetPixels(GalaxyRenderer.Render(
                    Viewport.Lerp(from, to, t), 1000, 1000,
                    GalaxyRenderer.DefaultColorMapping,
                    i == 0 ? (float?)null : previousMaxDensity,
                    out previousMaxDensity,
                    ref coordinates));
                if (!(image is null))
                {
                    image.Save(@"frames\" + i.ToString("000000") + ".png");
                    image.Dispose();
                }
                progress++;

                #region Logging progress per frame
                Console.WriteLine(progress + " / " + (frameRate * timeToTake) + "  -  " + ((progress * 100) / (double)(frameRate * timeToTake)).ToString("0.00") + "%");

                var secondsPerFrame = watch.Elapsed.TotalSeconds / progress;
                var framesRemaining = (frameRate * timeToTake) - progress;
                var timeRemaining = new TimeSpan((long)(10000000 * (framesRemaining * secondsPerFrame)));
                Console.WriteLine("Estimated time remaining: " +
                    timeRemaining.Hours.ToString("00") + ":" +
                    timeRemaining.Minutes.ToString("00") + ":" +
                    timeRemaining.Seconds.ToString("00"));
                Console.WriteLine("Average time per frame: " + secondsPerFrame.ToString("0.00") + " seconds.");
                #endregion
            }

            Console.WriteLine("Frame rendering complete. Compiling video...");
            var info = new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = string.Format(" -framerate {0} -i %06d.png -c:v libx264 -r {0} {1}.mp4", frameRate, "out"),
                WorkingDirectory = "frames",
            };
            var p = Process.Start(info);
            p.WaitForExit();
            Console.WriteLine("Finished. Saved video as out.mp4.");
            Console.Read();
        }

        public static float Sigmoid(float t) => (float)(1f / (1f + Math.Pow(2.71828d, -t)));
        public static float Smoothing(float t) => Sigmoid((10f * t) - 5f);
    }
}
