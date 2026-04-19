using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CRMClient = MiniCRM.Core.Models.CRMClient;

namespace MiniCRM.Client.Interop
{
    public static class ClientFilterInterop
    {
        private const int MaxStringLength = 99;
        private const int BufferSize = 100;

        public static void FilterAsync(
            List<CRMClient> clients, 
            string query,
            Action <List<CRMClient>> onComplete,
            Action <Exception> onError

            )
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = Filter(clients, query);
                    onComplete.Invoke(result);
                }
                catch (Exception ex)
                {
                    onError(ex);
                }
            });
        }

        public static List<CRMClient> Filter(List<CRMClient> clients, string query)
        {
            if (clients == null || clients.Count == 0)
                return new List<CRMClient>();

            if (string.IsNullOrWhiteSpace(query))
                return clients;

            var records = ToRecords(clients);

            var indices = new int[records.Length];
            var count = 0;

            var success = FilterClients(
                records,
                records.Length,
                query,
                indices,
                indices.Length,
                ref count);

            if (!success || count == 0)
                return new List<CRMClient>();

            return CollectResult(clients, indices, count);
        }

        private static ClientRecord[] ToRecords(List<CRMClient> clients)
        {
            var records = new ClientRecord[clients.Count];

            for (int i = 0; i < clients.Count; i++)
                records[i] = ToRecord(clients[i]);

            return records;
        }

        private static ClientRecord ToRecord(CRMClient client)
        {
            return new ClientRecord
            {
                Id = client.Id,
                FullName = Trim(client.FullName),
                Company = Trim(client.Company),
                Email = Trim(client.Email),
                Status = (int)client.Status
            };
        }

        private static List<CRMClient> CollectResult(
            List<CRMClient> clients,
            int[] indices,
            int count
            )
        {
            var result = new List<CRMClient>(count);

            for (int i = 0; i < count; i++)
                result.Add(clients[indices[i]]);

            return result;
        }

        private static string Trim(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length <= MaxStringLength)
                return value;

            return value.Substring(0, MaxStringLength);
        }

        [StructLayout(LayoutKind.Sequential,
            Pack = 1,
            CharSet = CharSet.Unicode)]
        private struct ClientRecord
        {
            public int Id;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BufferSize)]
            public string FullName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BufferSize)]
            public string Company;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BufferSize)]
            public string Email;

            public int Status;
        }

        [DllImport("ClientFilter.dll",
            CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode,
            EntryPoint = "FilterClients")]
        private static extern bool FilterClients(
            [In] ClientRecord[] records,
            int count,
            string query,
            [Out] int[] outIndices,
            int maxOut,
            ref int outCount
        );
    }
}