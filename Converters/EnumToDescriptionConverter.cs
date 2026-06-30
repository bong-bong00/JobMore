using System.Globalization;
using System.Windows.Data;
using JobMore.ViewModels;

namespace JobMore.Converters
{
    /// <summary>enum 값 → [Description] 한글 문자열 (바인딩 표시용).</summary>
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Enum e ? EnumHelper.GetDescription(e) : (value?.ToString() ?? string.Empty);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
