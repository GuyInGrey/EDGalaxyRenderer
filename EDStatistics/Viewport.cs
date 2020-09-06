using System;
using Processing;

namespace EDStatistics
{
    public class Viewport
    {
        public double Top;
        public double Bottom;
        public double Left;
        public double Right;

        public static Viewport Lerp(Viewport a, Viewport b, double t)
        {
            return new Viewport()
            {
                Top = Lerp(a.Top, b.Top, t),
                Bottom = Lerp(a.Bottom, b.Bottom, t),
                Left = Lerp(a.Left, b.Left, t),
                Right = Lerp(a.Right, b.Right, t),
            }; 
        }
        public static double Lerp(double a, double b, double t) =>
            a * (1 - t) + b * t;
    }
}
