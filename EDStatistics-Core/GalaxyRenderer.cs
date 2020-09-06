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

        public static PSprite Render(Viewport port, int width, int height, 
            PColor[] colorMap, Func<double, double> smoothing, double? previousMaxDensity, out double currentMaxDensity,
            string binPath)
        {
            var image = new PSprite(width, height);

            image.Art.Background(PColor.Black);

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

                    //var buffer_shader = Gpu.Default.AllocateReadWriteBuffer(buffer.ToList().ConvertAll(b => (int)b).ToArray());
                    //var density_shader = Gpu.Default.AllocateReadWriteBuffer<int>(width * height);
                    //var maxDensity_shader = Gpu.Default.AllocateReadWriteBuffer<int>(1);

                    for (var i = 0; i < systemsToRead; i++)
                    {
                        var index = (i * Converter.systemByteSize) + sizeof(long);
                        var x = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        /* var y = BitConverter.ToDouble(buffer, index); */
                        index += sizeof(double);
                        var z = BitConverter.ToDouble(buffer, index); index += sizeof(double);
                        //var name = Encoding.ASCII.GetString(buffer, index, 50); index += 50;
                        //Console.WriteLine(name + " (" + x + ", " + y + ", " + z + ")");

                        var pX = (int)Math.Floor(((x - port.Left) / rl) * width);
                        var pY = (int)Math.Floor(((z - port.Top) / bt) * height);

                        if (pX < 0 || pX >= width || pY < 0 || pY >= height) { continue; }
                        density[pX, pY]++;
                        if (density[pX, pY] > maxDensity) { maxDensity = density[pX, pY]; }
                    }

                    //Gpu.Default.For((int)systemsToRead, new SystemsCalculationShader(
                    //    buffer_shader, density_shader, width, height, maxDensity_shader, Converter.systemByteSize, port.Top, port.Bottom, port.Left, port.Right));

                    //var density_temp = new int[width * height];
                    //density_shader.GetData(density_temp);

                    //for (var i = 0; i < density_temp.Length; i++)
                    //{
                    //    density[i % width, (i - (i % width)) / width] = density_temp[i];
                    //}
                    //var maxDensity_temp = new int[1];
                    //maxDensity_shader.GetData(maxDensity_temp);
                    //maxDensity = maxDensity_temp[0];

                    buffer = null;
                }
            }

            var den = previousMaxDensity ?? maxDensity;
            //var den = (double)maxDensity;
            currentMaxDensity = den;

            var pixels = image.Art.GetPixels();
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

        //public (int, int) CoordinateToScreenSpace(float x, float y, float y)
        //{

        //}
    }
}
