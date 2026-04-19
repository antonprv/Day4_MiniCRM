using System.Runtime.Serialization;

namespace MiniCRM.Core.Contracts
{
    [DataContract]
    public class ClientFault
    {
        [DataMember] public string Code { get; set; }
        [DataMember] public string Message { get; set; }
    }
}
