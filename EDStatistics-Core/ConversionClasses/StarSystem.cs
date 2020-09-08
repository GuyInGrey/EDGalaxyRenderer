using System;
using System.Globalization;

namespace EDStatistics_Core
{
    [Serializable]
    public class StarSystem
    {
        public int id;
        public long id64;
        public string name;
        public Coordinates coords;
        public string date;
        public DateTime dateTime => DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
