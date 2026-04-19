using System.ComponentModel;

namespace MiniCRM.Core.Models
{
    public enum CRMClientStatus
    {
        [Description("Лид")]
        Lead = 0,
        [Description("Активный")]
        Active = 1,
        [Description("Неактивный")]
        Inactive = 2
    }
}