using System;
using ComputeSharp;
using Processing;

namespace EDStatistics_Core
{
    public class LiveGalaxyDisplay : ProcessingCanvas
    {
        PSprite image;
        Viewport currentView;
        double maxDensity;
        ReadOnlyBuffer<double> coordinates;

        Coordinates galMin = new Coordinates(-42213.81f, -29359.81f, -23405f);
        Coordinates galMax = new Coordinates(40503.81f, 39518.34f, 65630.16f);

        public LiveGalaxyDisplay(ref ReadOnlyBuffer<double> coordinates)
        {
            this.coordinates = coordinates;
            currentView = new Viewport(galMax.z, galMin.z, galMax.x, galMin.x);
            CreateCanvas(1000, 1000, 15);
        }

        public void Setup()
        {
            Render(true);
            var moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3;

            AddKeyAction("E", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("Q", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; ; });
            AddKeyAction("W", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("S", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("A", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });
            AddKeyAction("D", (t) => { moveFraction = Math.Abs(currentView.Top - currentView.Bottom) / 3; });

            AddKeyAction("E", (t) => { if (!t) { return; } currentView *= 0.9d; });
            AddKeyAction("Q", (t) => { if (!t) { return; } currentView *= 1.1d; });
            AddKeyAction("W", (t) => { if (!t) { return; } currentView += new Viewport(moveFraction, moveFraction, 0, 0); });
            AddKeyAction("S", (t) => { if (!t) { return; } currentView += new Viewport(-moveFraction, -moveFraction, 0, 0); });
            AddKeyAction("A", (t) => { if (!t) { return; } currentView += new Viewport(0, 0, -moveFraction, -moveFraction); });
            AddKeyAction("D", (t) => { if (!t) { return; } currentView += new Viewport(0, 0, moveFraction, moveFraction); });
        }

        public void Render(bool first)
        {
            image = GalaxyRenderer.Render(currentView, Width, Height,
                GalaxyRenderer.DefaultColorMapping, GalaxyRenderer.DefaultSmoothing,
                first ? (double?)null : maxDensity, out maxDensity, ref coordinates);
        }

        public void Draw(float delta)
        {
            if (image is null) { return; }
            Render(false);
            Art.SetPixels(image.Art.GetPixels());
            Title("Viewport: " + currentView);
            //Art.DrawImage(image, 0, 0, Width, Height);
            //Title("FPS: " + FrameRateCurrent);
        }
    }
}