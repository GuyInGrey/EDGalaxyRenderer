using System;
using System.IO;
using Processing;

namespace EDStatistics
{
    public static class GalaxyRenderer
    {
        public static PColor[] DefaultColorMapping = new[]
        {
            new PColor(0, 0, 0),
            new PColor(0, 150, 255),
            new PColor(0, 255, 0),
            new PColor(255, 255, 0),
            new PColor(255, 0, 255),
            new PColor(255, 0, 0),
        };

        public static PSprite Render(Viewport port, int width, int height, PColor[] colorMap)
        {
            var image = new PSprite(width, height);

            image.Art.Background(PColor.Black);
            var pixels = image.Art.GetPixels();

            var systemCount = (new FileInfo("systems.bin")).Length / 70;
            var systemBufferSizeTarget = 50000;

            var rl = port.Right - port.Left;
            var bt = port.Bottom - port.Top;

            var currentSystemIndex = 0L;

            var density = new int[width, height];
            var maxDensity = 0;

            using (var fileStream = new FileStream("systems.bin", FileMode.Open, FileAccess.Read))
            {
                while (systemCount > 0)
                {
                    var systemsToRead = systemCount < systemBufferSizeTarget ? systemCount : systemBufferSizeTarget;
                    systemCount -= systemsToRead;

                    var buffer = new byte[systemsToRead * 70];
                    fileStream.Read(buffer, 0, (int)systemsToRead * 70);
                    currentSystemIndex += systemsToRead;

                    for (var i = 0; i < systemsToRead; i++)
                    {
                        var index = (int)((i * 70) + 8);
                        var x = BitConverter.ToSingle(buffer, index);
                        var z = BitConverter.ToSingle(buffer, index + 8);

                        var pX = (int)Math.Round(((x - port.Left) / rl) * width);
                        var pY = (int)Math.Round(((z - port.Top) / bt) * height);

                        if (pX < 0 || pX >= width || pY < 0 || pY >= height) { continue; }
                        density[pX, pY]++;
                        if (density[pX, pY] > maxDensity) { maxDensity = density[pX, pY]; }
                    }
                    buffer = null;
                }
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (density[x, y] == 0) { continue; }
                    var address = (x + (y * width)) * 4;
                    if (!(address >= pixels.Length || address < 0))
                    {
                        var colorPercent = density[x, y] / (float)maxDensity;
                        colorPercent = -(float)Math.Pow(colorPercent - 1, 24f) + 1;
                        var color = PColor.LerpMultiple(colorMap, colorPercent > 1 ? 1 : colorPercent);
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
