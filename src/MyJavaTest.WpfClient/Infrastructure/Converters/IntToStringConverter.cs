using MyJavaTest.WpfClient.Infrastructure.Converters.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Infrastructure.Converters
{
    public class IntToStringConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
                return number.ToString();
            else
                return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
                return System.Convert.ToInt32(str);
            else
                return 0;
        }
    }
}
