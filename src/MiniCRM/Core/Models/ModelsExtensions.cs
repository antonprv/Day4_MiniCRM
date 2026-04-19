using System.ComponentModel;
using System.Reflection;

namespace MiniCRM.Core.Models
{
    public static class ModelsExtensions
    {
        public static string GetDescription(this ClientStatus value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
