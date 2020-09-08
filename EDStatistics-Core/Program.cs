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
        public static string BinPath => "systems-format3.bin";

        // This is the total range the coordinates can be in, so we can a good idea of a default viewport.
        static Coordinates galMin = new Coordinates(-42213.81f, -29359.81f, -23405f);
        static Coordinates galMax = new Coordinates(40503.81f, 39518.34f, 65630.16f);

        static void Main(string[] args)
        {
            if (!File.Exists(BinPath))
            {
                if (!File.Exists("systemsWithCoordinates.json"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No binary file was found, and no systems json file was found to convert.\n" +
                        "Please place the systemsWithCoordinates.json in the same folder as this executable.");
                    return;
                }

                Converter.ConvertExplorationData("systemsWithCoordinates.json", BinPath);
                Console.WriteLine("Conversion complete.");
                Console.Read();
                return;
            }
            var coords = Converter.GetCoordinates(BinPath);
            var coords2 = Gpu.Default.AllocateReadOnlyBuffer(coords);
            new LiveGalaxyDisplay(ref coords2);

            //// For animating the frames, I lerp between viewpoints for each frame. These are the 2 keys that the lerping follows.
            //var from = new Viewport(galMax.z, galMin.z, galMax.x, galMin.x);
            ////var to = new Viewport(10, -10, 10, -10);
            //var to = from;
        }
    }
}