using System;
using System.ServiceModel;
using MiniCRM.Service.Services;

namespace MiniCRM.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(ClientService));

            host.AddServiceEndpoint(
                typeof(MiniCRM.Core.Contracts.IClientService),
                new NetTcpBinding(),
                "net.tcp://localhost:8080/ClientService");

            host.Open();
            Console.WriteLine("Сервис запущен: net.tcp://localhost:8080/ClientService");
            Console.WriteLine("Нажми Enter для остановки...");
            Console.ReadLine();
            host.Close();
        }
    }
}