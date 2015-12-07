﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    class TimePassedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string v = (string)value;
            DateTime dt = System.Convert.ToDateTime(v);
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second" : ts.Seconds + " seconds";
            }
            if (delta < 120)
            {
                return "a minute";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = System.Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month" : months + " months";
            }
            int years = System.Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year" : years + " years";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
