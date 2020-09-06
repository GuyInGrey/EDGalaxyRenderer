using System;
using System.IO;
using System.Linq;
using ComputeSharp;
using Processing;

namespace EDStatistics_Core
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

        public static Func<double, double> DefaultSmoothing => new Func<double, double>((f) => -Math.Pow(f - 1, 24d) + 1);

        public static PSprite Render(Viewport port, int width, int height,
            PColor[] colorMap, Func<double, double> smoothing, double? previousMaxDensity, out double currentMaxDensity,
            ref ReadOnlyBuffer<double> coordinates)
        {
            var image = new PSprite(width, height);
            image.Art.Background(PColor.Black);

            var systemCount = coordinates.Size / 3;

            var density = new int[width * height];
            var maxDensity = 0;

            var top = port.Top;
            var bottom = port.Bottom;
            var right = port.Right;
            var left = port.Left;

            var density_shader = Gpu.Default.AllocateReadWriteBuffer<int>(width * height);
            var maxDensity_shader = Gpu.Default.AllocateReadWriteBuffer<int>(1);

            var dispatchSize = 128;

            var shader = new SystemsCalculationShader(
                coordinates,
                density_shader,
                width,
                height,
                maxDensity_shader,
                port.Top,
                port.Bottom,
                port.Left,
                port.Right,
                dispatchSize,
                coordinates.Size);

            Gpu.Default.For((coordinates.Size / 3) / dispatchSize, shader);
            density = density_shader.GetData();
            maxDensity = maxDensity_shader.GetData()[0];

            density_shader.Dispose();
            maxDensity_shader.Dispose();

            //for (var i = 0; i < coordinates.Length / 3; i++)
            //{
            //    var pX = (int)Math.Floor(((coordinates[i * 3] - left) / (right - left)) * width);
            //    var pY = (int)Math.Floor(((coordinates[(i * 3) + 2] - top) / (bottom - top)) * height);
            //    if (pX < 0 || pX >= width || pY < 0 || pY >= height) { continue; }
            //    density[pX, pY]++;
            //    if (density[pX, pY] > maxDensity) { maxDensity = density[pX, pY]; }
            //}

            var den = previousMaxDensity ?? maxDensity;
            currentMaxDensity = den;

            var pixels = image.Art.GetPixels();
            for (var i = 0; i < density.Length; i++)
            {
                if (density[i] == 0) { continue; }
                var address = i * 4;
                var colorPercent = density[i] / den;
                colorPercent = smoothing(colorPercent);
                var color = PColor.LerpMultiple(colorMap, (float)(colorPercent > 1 ? 1 : colorPercent));
                pixels[address + 0] = (byte)color.B;
                pixels[address + 1] = (byte)color.G;
                pixels[address + 2] = (byte)color.R;
                pixels[address + 3] = 255;
            }
            image.Art.SetPixels(pixels);

            return image;
        }
    }
}
