using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkDistributionApp.Controllers
{
    public class TimeConverter
    {
        public static DateTime ConvertToLocalTime(DateTime utcTime, string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return utcTime;
            }
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcTime, timeZoneId);
        }
    }
}