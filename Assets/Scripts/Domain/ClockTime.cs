using System;
using System.Globalization;

namespace LastTrainHome.Domain
{
    /// <summary>A time of night, stored as whole minutes after midnight.</summary>
    public readonly struct ClockTime : IEquatable<ClockTime>, IComparable<ClockTime>
    {
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;

        public ClockTime(int minutesAfterMidnight)
        {
            if (minutesAfterMidnight < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minutesAfterMidnight), minutesAfterMidnight, "Time cannot be before midnight.");
            }

            MinutesAfterMidnight = minutesAfterMidnight;
        }

        public int MinutesAfterMidnight { get; }

        /// <summary>Reads "HH:MM" after midnight, e.g. "00:40" or "01:00".</summary>
        public static bool TryParse(string text, out ClockTime time)
        {
            time = default;
            string[] parts = text?.Split(':');
            if (parts == null
                || parts.Length != 2
                || parts[1].Length != 2
                || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int hours)
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minutes)
                || minutes >= MinutesPerHour)
            {
                return false;
            }

            time = new ClockTime(hours * MinutesPerHour + minutes);
            return true;
        }

        public ClockTime AddMinutes(int minutes) => new ClockTime(MinutesAfterMidnight + minutes);

        /// <summary>Minutes from <paramref name="earlier"/> to this time. Negative if this time is earlier.</summary>
        public int MinutesSince(ClockTime earlier) => MinutesAfterMidnight - earlier.MinutesAfterMidnight;

        /// <summary>24-hour clock, e.g. "00:40".</summary>
        public override string ToString()
        {
            int hours = MinutesAfterMidnight / MinutesPerHour % HoursPerDay;
            int minutes = MinutesAfterMidnight % MinutesPerHour;
            return hours.ToString("00", CultureInfo.InvariantCulture) + ":" +
                   minutes.ToString("00", CultureInfo.InvariantCulture);
        }

        public bool Equals(ClockTime other) => MinutesAfterMidnight == other.MinutesAfterMidnight;

        public override bool Equals(object obj) => obj is ClockTime other && Equals(other);

        public override int GetHashCode() => MinutesAfterMidnight;

        public int CompareTo(ClockTime other) => MinutesAfterMidnight.CompareTo(other.MinutesAfterMidnight);

        public static bool operator ==(ClockTime left, ClockTime right) => left.Equals(right);

        public static bool operator !=(ClockTime left, ClockTime right) => !left.Equals(right);

        public static bool operator <(ClockTime left, ClockTime right) => left.CompareTo(right) < 0;

        public static bool operator >(ClockTime left, ClockTime right) => left.CompareTo(right) > 0;

        public static bool operator <=(ClockTime left, ClockTime right) => left.CompareTo(right) <= 0;

        public static bool operator >=(ClockTime left, ClockTime right) => left.CompareTo(right) >= 0;
    }
}
