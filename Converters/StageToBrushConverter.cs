using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using JobMore.Models;

namespace JobMore.Converters
{
    /// <summary>전형 단계 → 배지 색상.</summary>
    public class StageToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Stage s) return Brushes.Gray;
            return s switch
            {
                Stage.Interested      => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
                Stage.Applied         => new SolidColorBrush(Color.FromRgb(0x42, 0xA5, 0xF5)),
                Stage.DocumentPassed  => new SolidColorBrush(Color.FromRgb(0x29, 0xB6, 0xF6)),
                Stage.FirstInterview  => new SolidColorBrush(Color.FromRgb(0xAB, 0x47, 0xBC)),
                Stage.SecondInterview => new SolidColorBrush(Color.FromRgb(0x8E, 0x24, 0xAA)),
                Stage.Negotiation     => new SolidColorBrush(Color.FromRgb(0xFF, 0xA7, 0x26)),
                Stage.Offer           => new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A)),
                Stage.Rejected        => new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)),
                Stage.Withdrawn       => new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD)),
                _                     => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
