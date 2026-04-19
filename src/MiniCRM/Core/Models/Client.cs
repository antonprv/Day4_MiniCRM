using System;
using System.Runtime.Serialization;

namespace MiniCRM.Core.Models
{
    [DataContract]
    public class Client
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string FullName { get; set; }
        [DataMember] public string Phone { get; set; }
        [DataMember] public string Email { get; set; }
        [DataMember] public string Company { get; set; }
        [DataMember] public ClientStatus Status { get; set; }
        [DataMember] public DateTime CreatedAt { get; set; }
    }
}
