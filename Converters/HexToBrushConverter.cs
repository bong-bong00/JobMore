using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JobMore.Converters
{
    /// <summary>"#RRGGBB" 문자열 → Brush. 비어있으면 회색 폴백.</summary>
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var hex = value as string;
            if (string.IsNullOrWhiteSpace(hex)) hex = "#9AA3B2";
            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(0x9A, 0xA3, 0xB2));
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
