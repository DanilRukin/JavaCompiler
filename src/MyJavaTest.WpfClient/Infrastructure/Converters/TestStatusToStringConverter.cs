using MyJavaTest.WpfClient.Infrastructure.Converters.Base;
using MyJavaTest.WpfClient.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyJavaTest.WpfClient.Infrastructure.Converters
{
    public class TestStatusToStringConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestStatus testStatus)
            {
                return testStatus switch
                {
                    TestStatus.Success => "Успешно",
                    TestStatus.InProcessing => "Тестируется",
                    TestStatus.NotTested => "Не протестировано",
                    TestStatus.Error => "С ошибками",
                    _ => "",
                };
            }
            else
                return string.Empty;
        }
    }
}
