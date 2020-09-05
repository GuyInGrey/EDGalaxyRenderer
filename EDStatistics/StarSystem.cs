using System;

namespace EDStatistics
{
    [Serializable]
    public class StarSystem
    {
        public int id;
        public long id64;
        public string name;
        public Coordinates coords;
        public string date;
    }
}
