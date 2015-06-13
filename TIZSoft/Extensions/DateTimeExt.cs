using System;
using Tizsoft.Helpers;

namespace Tizsoft.Extensions
{
    public static class DateTimeExt
    {
        public static string ToMysqlTimeStampString(this DateTime dateTime)
        {
            return dateTime.ToString(Utils.MysqlTimeStampFormat);
        }

        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            return (long)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static long UtcNowToUtcTimeStamp(this DateTime dateTime)
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}