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
    }
}