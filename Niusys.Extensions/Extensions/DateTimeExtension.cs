using System;

namespace Niusys
{
    public static class DateTimeExtension
    {
        public static long GetEpochSeconds(this DateTime date)
        {
            TimeSpan t = TimeZoneInfo.ConvertTimeToUtc(date) - new DateTime(1970, 1, 1);
            return (long)t.TotalSeconds;
        }

        public static long GetEpochMilliseconds(this DateTime date)
        {
            TimeSpan t = TimeZoneInfo.ConvertTimeToUtc(date) - new DateTime(1970, 1, 1);
            return (long)t.TotalMilliseconds;
        }

        public static DateTime FromEpochSeconds(this long EpochSeconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(EpochSeconds);
        }

        public static DateTime FromEpochMilliseconds(this long EpochMilliseconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(EpochMilliseconds);
        }

        public static DateTime ToChinaStandardTime(this DateTime datetime)
        {
            if (datetime.Kind == DateTimeKind.Utc)
            {
                return datetime.AddHours(8);
            }
            return datetime;
        }
    }
}
