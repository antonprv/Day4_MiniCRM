using System;
using System.Collections.Generic;
using System.ServiceModel;

using MiniCRM.Core.Contracts;
using MiniCRM.Core.Models;

namespace MiniCRM.Client.Services
{
    public class CrmServiceClient : IDisposable
    {
        private readonly ChannelFactory<IClientService> _factory;
        private readonly IClientService _channel;

        public CrmServiceClient()
        {
            var binding = new NetTcpBinding();
            var endpoint = new EndpointAddress("net.tcp://localhost:8080/ClientService");
            _factory = new ChannelFactory<IClientService>(binding, endpoint);
            _channel = _factory.CreateChannel();
        }

        public List<CRMClient> GetAllClients()
        {
            return _channel.GetAllClients();
        }

        public CRMClient GetClientById(int id)
        {
            return _channel.GetClientById(id);
        }

        public int AddClient(CRMClient client)
        {
            return _channel.AddClient(client);
        }

        public void UpdateClient(CRMClient client)
        {
            _channel.UpdateClient(client);
        }

        public void DeleteClient(int id)
        {
            _channel.DeleteClient(id);
        }

        public List<CRMClient> SearchClients(string query)
        {
            return _channel.SearchClients(query);
        }

        public void Dispose()
        {
            try
            {
                if (_factory.State != CommunicationState.Faulted)
                    _factory.Close();
                else
                    _factory.Abort();
            }
            catch
            {
                _factory.Abort();
            }
        }
    }
}