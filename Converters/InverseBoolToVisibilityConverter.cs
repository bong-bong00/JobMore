using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JobMore.Converters
{
    /// <summary>true → Collapsed, false → Visible (BoolVis의 반대).</summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool v && v;
            return b ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
            => value is Visibility vis && vis != Visibility.Visible;
    }
}
