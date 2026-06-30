using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JobMore.Converters
{
    /// <summary>두 값(팔레트 색, 현재 선택 색)이 같으면 Visible.</summary>
    public class ColorMatchToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return Visibility.Collapsed;
            var a = values[0] as string;
            var b = values[1] as string;
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
            => null;
    }
}
