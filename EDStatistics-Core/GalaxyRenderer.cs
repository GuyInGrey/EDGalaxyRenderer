using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        public static byte[] Render(Viewport port, int width, int height,
            PColor[] colorMap, float? previousMaxDensity, out float currentMaxDensity,
            ref ReadOnlyBuffer<float> coordinates)
        {
            try
            {
                var systemCount = coordinates.Size / 3;

                var density = new int[width * height];
                var maxDensity = 0;

                var top = port.Top;
                var bottom = port.Bottom;
                var right = port.Right;
                var left = port.Left;

                var dispatchSize = 8192;

                var density_shader = Gpu.Default.AllocateReadWriteBuffer<int>(width * height);
                var maxDensity_shader = Gpu.Default.AllocateReadWriteBuffer<int>(1);

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

                var den = previousMaxDensity ?? maxDensity;
                currentMaxDensity = den;

                //var pixels = image.Art.GetPixels();
                //for (var i = 0; i < density.Length; i++)
                //{
                //    if (density[i] == 0) { continue; }
                //    var address = i * 4;
                //    var colorPercent = density[i] / den;
                //    colorPercent = smoothing(colorPercent);
                //    var color = PColor.LerpMultiple(colorMap, (float)(colorPercent));
                //    pixels[address + 0] = (byte)color.B;
                //    pixels[address + 1] = (byte)color.G;
                //    pixels[address + 2] = (byte)color.R;
                //    pixels[address + 3] = 255;
                //}

                var pixels_shader = Gpu.Default.AllocateReadWriteBuffer<int>(width * height * 4);
                var shader2 = new HeatmapImageGenerationShader(density_shader, pixels_shader, width, den);
                Gpu.Default.For(height, shader2);
                var data = pixels_shader.GetData();

                pixels_shader.Dispose();
                density_shader.Dispose();
                maxDensity_shader.Dispose();
                //return Array.ConvertAll(data, i => (byte)i);
                //return StructureToByteArray(data);
                return Convert(data);
                //return new byte[data.Length];
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error (C)!\n" + e.Message + "\n" + e.StackTrace);
                currentMaxDensity = 0;
                return null;
            }
        }

        private static byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);

            var array = new byte[length];

            var pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

        public static byte[] Convert(int[] data)
        {
            var result = new byte[data.Length];
            Parallel.For(0, data.Length, i =>
            {
                result[i] = (byte)data[i];
            });
            return result;
        }
    }
}
