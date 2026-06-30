using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JobMore.Converters
{
    /// <summary>double → GridLength(value, Star). 퍼널 막대 비율용.</summary>
    public class DoubleToStarConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            double v = 0;
            if (value is double d) v = d;
            else if (value is int i) v = i;
            return new GridLength(v < 0 ? 0 : v, GridUnitType.Star);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
