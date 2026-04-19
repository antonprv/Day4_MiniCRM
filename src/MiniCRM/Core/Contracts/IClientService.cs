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
        List<Client> GetAllClients();

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        Client GetClientById(int id);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        int AddClient(Client client);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        void UpdateClient(Client client);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        void DeleteClient(int id);

        [OperationContract]
        [FaultContract(typeof(ClientFault))]
        List<Client> SearchClients(string query);
    }
}