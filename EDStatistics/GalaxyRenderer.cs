using System;
using System.IO;
using System.Text;
using Processing;

namespace EDStatistics
{
    public static class GalaxyRenderer
    {
        public static PColor[] DefaultColorMapping = new[]
        {
            new PColor(0, 5, 5),
            new PColor(0, 150, 255),
            new PColor(0, 255, 0),
            new PColor(255, 255, 0),
            new PColor(255, 0, 255),
            new PColor(255, 0, 0),
        };

        public static Func<double, double> DefaultSmoothing => new Func<double, double>((f) => -Math.Pow(f - 1, 24d) + 1);

        public static PSprite Render(Viewport port, int width, int height, 
            PColor[] colorMap, Func<double, double> smoothing, double? previousMaxDensity, out double currentMaxDensity,
            string binPath)
        {
            var image = new PSprite(width, height);

            image.Art.Background(PColor.Black);
            var pixels = image.Art.GetPixels();

            var systemCount = (new FileInfo(binPath)).Length / Converter.systemByteSize;
            var systemBufferSizeTarget = 500000;

            var rl = port.Right - port.Left;
            var bt = port.Bottom - port.Top;

            var density = new int[width, height];
            var maxDensity = 0;

            using (var fileStream = new FileStream(binPath, FileMode.Open, FileAccess.Read))
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
                        /* var y = BitConverter.ToDouble(buffer, index); */ index += sizeof(double);
                        var z = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        //var name = Encoding.ASCII.GetString(buffer, index, 50); index += 50;
                        //Console.WriteLine(name + " (" + x + ", " + y + ", " + z + ")");

                        var pX = (int)Math.Floor(((x - port.Left) / rl) * width);
                        var pY = (int)Math.Floor(((z - port.Top) / bt) * height);

                        if (pX < 0 || pX >= width || pY < 0 || pY >= height) { continue; }
                        density[pX, pY]++;
                        if (density[pX, pY] > maxDensity) { maxDensity = density[pX, pY]; }
                    }
                    buffer = null;
                }
            }

            var den = previousMaxDensity ?? maxDensity;
            //var den = (double)maxDensity;
            currentMaxDensity = den;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (density[x, y] == 0) { continue; }
                    var address = (x + (y * width)) * 4;
                    if (!(address >= pixels.Length || address < 0))
                    {
                        var colorPercent = density[x, y] / den;
                        colorPercent = smoothing(colorPercent);
                        var color = PColor.LerpMultiple(colorMap, (float)(colorPercent > 1 ? 1 : colorPercent));
                        pixels[address + 0] = (byte)color.B;
                        pixels[address + 1] = (byte)color.G;
                        pixels[address + 2] = (byte)color.R;
                        pixels[address + 3] = 255;
                    }
                }
            }
            image.Art.SetPixels(pixels);

            return image;
        }
    }
}
