using System;
using System.Linq;
using ComputeSharp;
using Processing;

namespace EDStatistics_Core
{
    public class LiveGalaxyDisplay : ProcessingCanvas
    {
        Viewport currentView;
        float maxDensity;
        public static ReadOnlyBuffer<float> coordinates;

        Coordinates galMin = new Coordinates(-42213.81f, -29359.81f, -23405f);
        Coordinates galMax = new Coordinates(40503.81f, 39518.34f, 65630.16f);

        public LiveGalaxyDisplay(ref ReadOnlyBuffer<float> coords)
        {
            coordinates = coords;
            currentView = new Viewport(galMax.z, galMin.z, galMax.x, galMin.x);
            CreateCanvas(1000, 1000, 60);
        }

        public void Setup()
        {
            GalaxyRenderer.Render(currentView, Width, Height,
                GalaxyRenderer.DefaultColorMapping,
                null, out maxDensity, ref coordinates);

            var moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3;

            AddKeyAction("E", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("Q", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; ; });
            AddKeyAction("W", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("S", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("A", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("D", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });

            AddKeyAction("E", (t) => { if (!t) { return; } currentView *= 0.9f; });
            AddKeyAction("Q", (t) => { if (!t) { return; } currentView *= 1.1f; });
            AddKeyAction("W", (t) => { if (!t) { return; } currentView += new Viewport(moveFraction, moveFraction, 0, 0); });
            AddKeyAction("S", (t) => { if (!t) { return; } currentView += new Viewport(-moveFraction, -moveFraction, 0, 0); });
            AddKeyAction("A", (t) => { if (!t) { return; } currentView += new Viewport(0, 0, -moveFraction, -moveFraction); });
            AddKeyAction("D", (t) => { if (!t) { return; } currentView += new Viewport(0, 0, moveFraction, moveFraction); });

            AddKeyAction("X", (t) =>
            {
                if (!t) { return; }
                Form.Hide();
                coordinates.Dispose();
                coordinates = Gpu.Default.AllocateReadOnlyBuffer(Converter.GetCoordinates(Program.BinPath));
                Form.Show();
            });
        }

        public void Draw(float delta)
        {
            var i = GalaxyRenderer.Render(currentView, Width, Height,
                GalaxyRenderer.DefaultColorMapping,
                null, out maxDensity, ref coordinates);
            if (!(i is null))
            {
                Art.SetPixels(i);
            }
            Title("Viewport: " + currentView + " - " + 
                FrameRateCurrent + " fps - " + 
                TotalFrameCount);
            //GC.Collect();
            //Art.DrawImage(image, 0, 0, Width, Height);
            //Title("FPS: " + FrameRateCurrent);
        }
    }
}