using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JobMore.Converters
{
    /// <summary>D-day(int?) → 표시 텍스트. null이면 빈 칸.</summary>
    public class DdayToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int d) return string.Empty;
            if (d == 0) return "D-DAY";
            return d > 0 ? $"D-{d}" : $"D+{-d}";
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    /// <summary>D-day(int?) → 색상. 마감 3일 이내만 빨강, 그 외엔 대표색(인디고). 지난 일정은 회색.</summary>
    public class DdayToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int d) return Brushes.Transparent;
            if (d < 0)  return new SolidColorBrush(Color.FromRgb(0x9E, 0xA3, 0xB8)); // 지난 일정 - 회색
            if (d <= 3) return new SolidColorBrush(Color.FromRgb(0xD6, 0x45, 0x45)); // 임박(3일 이내) - 빨강
            return new SolidColorBrush(Color.FromRgb(0x52, 0x5C, 0xB0));             // 그 외 - 대표색(인디고)
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }
}
