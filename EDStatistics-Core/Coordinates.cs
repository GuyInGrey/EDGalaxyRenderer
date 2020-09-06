using System;

namespace EDStatistics_Core
{
    [Serializable]
    public class Coordinates
    {
        public double x;
        public double y;
        public double z;

        public Coordinates() { }
        public Coordinates(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
        public Coordinates(double x, double y) : this(x, y, 0) { }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }
    }
}
