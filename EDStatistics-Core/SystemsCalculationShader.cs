﻿using ComputeSharp;

namespace EDStatistics_Core
{
    public readonly struct SystemsCalculationShader : IComputeShader
    {
        public readonly ReadOnlyBuffer<float> coordinates;
        public readonly int coordinatesSize;
        public readonly ReadWriteBuffer<int> density;
        public readonly ReadWriteBuffer<int> maxDensity;

        private readonly int width;
        private readonly int height;
        private readonly float top;
        private readonly float bottom;
        private readonly float left;
        private readonly float right;
        private readonly int iterations;

        public SystemsCalculationShader(
                ReadOnlyBuffer<float> coordinates,
                ReadWriteBuffer<int> density,
                int width,
                int height,
                ReadWriteBuffer<int> maxDensity,
                float top,
                float bottom,
                float left,
                float right,
                int iterations,
                int coordinatesSize
        ) {
            this.coordinates = coordinates;
            this.density = density;
            this.width = width;
            this.height = height;
            this.maxDensity = maxDensity;
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
            this.iterations = iterations;
            this.coordinatesSize = coordinatesSize;
        }

        public void Execute(ThreadIds id)
        {
            var currentMax = 0;
            for (var j = 0; j < iterations; j++)
            {
                var i = (id.X * iterations) + j;
                if (i * 3 >= coordinatesSize) { return; }
                var pX = (int)Hlsl.Floor(((coordinates[i * 3] - left) / (right - left)) * width);
                var pY = (int)Hlsl.Floor(((coordinates[(i * 3) + 2] - top) / (bottom - top)) * height);
                if (pX < 0 || pX >= width || pY < 0 || pY >= height) { continue; }
                var denIndex = pX + (pY * width);
                density[denIndex]++;
                if (density[denIndex] > currentMax) { currentMax = density[denIndex]; }
            }

            Hlsl.InterlockedMax(maxDensity[0], currentMax);
            //if (currentMax > maxDensity[0]) { maxDensity[0] = currentMax; }
        }
    }
}
