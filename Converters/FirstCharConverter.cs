using System;
using System.Globalization;
using System.Windows.Data;

namespace JobMore.Converters
{
    /// <summary>문자열의 첫 글자만 반환 (아바타 이니셜용).</summary>
    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s)) return "";
            return s.Trim().Substring(0, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
