using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HttpClientTester
{
    // Converts binded value to int when converting to int Property
    public class IntConverter : IValueConverter
    {
        // displays all ints on the textbox
        // displays an empty string if int is 0
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().Equals("0"))
            {
                return string.Empty;
            }
            return value.ToString();
        }

        // converts textbox input into an int
        // Empty string is a 0
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value.ToString();
            if (string.IsNullOrEmpty(val))
            {
                return 0;
            }
            else
            {
                return int.Parse(val);
            }
        }
    }
}
