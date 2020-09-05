using System;

namespace EDStatistics
{
    [Serializable]
    public class Coordinates
    {
        public float x;
        public float y;
        public float z;

        public Coordinates() { }
        public Coordinates(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Coordinates(float x, float y) : this(x, y, 0) { }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }
    }
}
