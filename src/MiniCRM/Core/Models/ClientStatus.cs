using System.ComponentModel;

namespace MiniCRM.Core.Models
{
    public enum ClientStatus
    {
        [Description("Лид")]
        Lead = 0,
        [Description("Активный")]
        Active = 1,
        [Description("Неактивный")]
        Inactive = 2
    }
}