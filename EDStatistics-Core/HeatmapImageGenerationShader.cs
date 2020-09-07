using System.Drawing;
using ComputeSharp;
using Windows.Devices.SmartCards;

namespace EDStatistics_Core
{
    public readonly struct HeatmapImageGenerationShader : IComputeShader
    {
        public readonly ReadWriteBuffer<int> density;
        public readonly ReadWriteBuffer<int> image;
        public readonly int width;
        public readonly double maxDensity;

        public HeatmapImageGenerationShader(
            ReadWriteBuffer<int> density,
            ReadWriteBuffer<int> image,
            int width,
            double maxDensity
        ) {
            this.density = density;
            this.image = image;
            this.width = width;
            this.maxDensity = maxDensity;
        }

        public void Execute(ThreadIds ids)
        {
            var y = ids.X;
            for (var x = 0; x < width; x++)
            {
                var j = (x + (y * width)) * 4;
                var value = density[x + (y * width)] / (float)maxDensity;
                if (value > 1f) { value = 1f; }
                if (value < 0f) { value = 0f; }
                //var v = value - 1;
                //value = Hlsl.Pow(v, 24);
                //value = -value + 1;
                //value = -(Hlsl.Pow(-value, 2)) + 1; //    THIS LINE
                image[j] = image[j + 1] = image[j + 2] = (int)(value * 255);
                image[j + 3] = 255;
            }
        }
    }
}
