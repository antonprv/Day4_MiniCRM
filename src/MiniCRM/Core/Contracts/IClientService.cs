using System.Collections.Generic;
using System.ServiceModel;

using MiniCRM.Core.Models;

namespace MiniCRM.Core.Contracts
{
    [ServiceContract]
    public interface IClientService
    {
        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        List<CRMClient> GetAllClients();

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        CRMClient GetClientById(int id);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        int AddClient(CRMClient client);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        void UpdateClient(CRMClient client);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        void DeleteClient(int id);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        List<CRMClient> SearchClients(string query);
    }
}