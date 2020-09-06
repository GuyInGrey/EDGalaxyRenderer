using System;
using Processing;

namespace EDStatistics_Core
{
    public class Viewport
    {
        public double Top;
        public double Bottom;
        public double Left;
        public double Right;

        public Viewport() : this(0, 0, 0, 0) { }

        public Viewport(double t, double b, double r, double l)
        {
            Top = t;
            Bottom = b;
            Right = r;
            Left = l;
        }

        public static Viewport Lerp(Viewport a, Viewport b, double t)
        {
            return new Viewport()
            {
                Top = Lerp(a.Top, b.Top, t),
                Bottom = Lerp(a.Bottom, b.Bottom, t),
                Right = Lerp(a.Right, b.Right, t),
                Left = Lerp(a.Left, b.Left, t),
            }; 
        }
        public static double Lerp(double a, double b, double t) =>
            a * (1 - t) + b * t;
    }
}
