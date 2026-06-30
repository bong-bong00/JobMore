using System.ComponentModel;
using System.Reflection;

namespace JobMore.ViewModels
{
    /// <summary>enum 멤버의 [Description("한글")] 값을 읽어오는 헬퍼.</summary>
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            if (value == null) return string.Empty;
            FieldInfo field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
