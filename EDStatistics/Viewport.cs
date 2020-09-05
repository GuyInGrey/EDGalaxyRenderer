using System;
using Processing;

namespace EDStatistics
{
    public class Viewport
    {
        public float Top;
        public float Bottom;
        public float Left;
        public float Right;

        public static Viewport Lerp(Viewport a, Viewport b, float t)
        {
            return new Viewport()
            {
                Top = PMath.Lerp(a.Top, b.Top, t),
                Bottom = PMath.Lerp(a.Bottom, b.Bottom, t),
                Left = PMath.Lerp(a.Left, b.Left, t),
                Right = PMath.Lerp(a.Right, b.Right, t),
            }; 
        }
    }
}
