using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class IntToMillionConverter: IValueConverter
    {

            public object Convert(object value, Type targetType, object parameter, string language)
            {
                int val = 0;
                try
                {
                    val = (int)value;
                }
                catch (Exception)
                {
                    return 0.ToString();
                }
                
                double res = 0;
                string s = "";
                if (val > 1000000)
                {
                    double d = val/1000000;
                    res = Math.Round(d,2);
                    s = "M";
                }
                else if (val > 1000)
                {
                    double d = val / 1000;
                    res = Math.Round(d, 2);
                    s = "K";
                }


                return res + s;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        
    }
}
