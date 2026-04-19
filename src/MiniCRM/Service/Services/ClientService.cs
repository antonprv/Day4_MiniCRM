using System;
using System.Collections.Generic;
using System.ServiceModel;
using MiniCRM.Core.Contracts;
using MiniCRM.Core.Models;
using MiniCRM.Service.Data;

namespace MiniCRM.Service.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ClientService : IClientService
    {
        // Один экземпляр DatabaseHelper на весь сервис
        private static readonly DatabaseHelper Db =
            new DatabaseHelper("crm.db");

        public List<Client> GetAllClients()
        {
            return Execute(() => Db.GetAll(), "GET_ALL");
        }

        public Client GetClientById(int id)
        {
            return Execute(() =>
            {
                var client = Db.GetById(id);
                if (client == null)
                    ThrowFault("NOT_FOUND", $"Клиент с Id={id} не найден");
                return client;
            }, "GET_BY_ID");
        }

        public int AddClient(Client client)
        {
            return Execute(() =>
            {
                ValidateClient(client);
                client.CreatedAt = DateTime.Now;
                return Db.Insert(client);
            }, "ADD");
        }

        public void UpdateClient(Client client)
        {
            Execute<object>(() =>
            {
                ValidateClient(client);
                Db.Update(client);
                return null;
            }, "UPDATE");
        }

        public void DeleteClient(int id)
        {
            Execute<object>(() =>
            {
                Db.Delete(id);
                return null;
            }, "DELETE");
        }

        public List<Client> SearchClients(string query)
        {
            return Execute(() =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Db.GetAll();
                return Db.Search(query);
            }, "SEARCH");
        }

        #region Helper Methods

        private void ValidateClient(Client client)
        {
            if (client == null)
                ThrowFault("INVALID", "Клиент не может быть null");
            if (string.IsNullOrWhiteSpace(client.FullName))
                ThrowFault("INVALID", "FullName обязателен");
        }

        private void ThrowFault(string code, string message)
        {
            throw new FaultException<ClientFault>(
                new ClientFault { Code = code, Message = message },
                new FaultReason(message));
        }

        // Обёртка чтобы не писать try/catch в каждом методе
        private T Execute<T>(Func<T> action, string operationName)
        {
            try
            {
                return action();
            }
            catch (FaultException)
            {
                throw; // FaultException пробрасываем как есть
            }
            catch (Exception ex)
            {
                throw new FaultException<ClientFault>(
                    new ClientFault
                    {
                        Code = "INTERNAL",
                        Message = $"Ошибка операции {operationName}: {ex.Message}"
                    },
                    new FaultReason(ex.Message));
            }
        }

        #endregion
    }
}