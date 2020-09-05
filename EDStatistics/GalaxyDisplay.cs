using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;
using Processing;

namespace EDStatistics
{
    public class GalaxyDisplay : ProcessingCanvas
    {
        public List<StarSystem> systems;


        public GalaxyDisplay()
        {
            CreateCanvas(1000, 1000, 30);
        }

        public void Setup()
        {

        }

        public void Draw(float delta)
        {
            Art.Background(PColor.Black);
            Title(FrameRateCurrent);
        }
    }
}