using System;
using System.Collections.Generic;
using System.Threading;

using MiniCRM.Client.Interop;
using MiniCRM.Client.Services;
using MiniCRM.Core.Models;

namespace MiniCRM.Client.Controllers
{
    public class ClientsController : IDisposable
    {
        private readonly object _sync = new object();
        private bool _disposed;
        private int _requestId;

        private List<CRMClient> _cache = new List<CRMClient>();

        private readonly Queue<Action> _queue = new Queue<Action>();

        public ClientsController()
        {
            var worker = new Thread(WorkerLoop)
            {
                IsBackground = true,    // умрёт вместе с приложением
                Name = "ClientsController.Worker"
            };
            worker.Start();
        }

        private void Enqueue(Action job)
        {
            lock (_sync)
            {
                _queue.Enqueue(job);
                Monitor.Pulse(_sync);
            }
        }

        private void WorkerLoop()
        {
            while (true)
            {
                Action job;

                lock (_sync)
                {
                    while (_queue.Count == 0 && !_disposed)
                        Monitor.Wait(_sync);

                    if (_disposed) return;

                    job = _queue.Dequeue();
                }

                try
                {
                    job();
                }
                catch { }
            }
        }

        /// <summary>
        /// Обновить кэш клиентов.
        /// </summary>
        public void LoadClients(Action<List<CRMClient>> onSuccess, Action<Exception> onError)
        {
            var id = Interlocked.Increment(ref _requestId);

            Enqueue(() =>
            {
                var svc = new CrmServiceClient();

                svc.GetAllClientsAsync(
                    onComplete: clients =>
                    {
                        if (id != _requestId) return; // выкидываем тухлый ответ

                        _cache = clients;
                        onSuccess?.Invoke(clients);
                        svc?.Dispose();
                    },
                    onError: ex =>
                    {
                        if (id != _requestId) return;
                        onError?.Invoke(ex);
                        svc?.Dispose();
                    });
            });
        }

        /// <summary>
        /// Фильтровать по кэшу (без обращения к серверу).
        /// </summary>
        public void Filter(string query, Action<List<CRMClient>> onSuccess, Action<Exception> onError = null)
        {
            var id = Interlocked.Increment(ref _requestId);

            // копируем кэш - безопасно читать из другого потока
            var snapshot = new List<CRMClient>(_cache);

            Enqueue(() =>
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var result = ClientFilterInterop.Filter(snapshot, query);

                        if (id != _requestId) return; // выкидываем тухлый ответ

                        onSuccess?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        if (id != _requestId) return;
                        onError?.Invoke(ex);
                    }
                });

                thread.IsBackground = true;
                thread.Start();
            });
        }

        /// <summary>Добавить клиента на сервер в фоне.</summary>
        public void AddClient(CRMClient client, Action onSuccess, Action<Exception> onError)
        {
            Enqueue(() =>
            {
                try
                {
                    using (var svc = new CrmServiceClient())
                        svc.AddClient(client);

                    onSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });
        }

        /// <summary>Обновить клиента на сервере в фоне.</summary>
        public void UpdateClient(CRMClient client, Action onSuccess, Action<Exception> onError)
        {
            Enqueue(() =>
            {
                try
                {
                    using (var svc = new CrmServiceClient())
                        svc.UpdateClient(client);

                    onSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });
        }

        /// <summary>Удалить клиента с сервера в фоне.</summary>
        public void DeleteClient(int clientId, Action onSuccess, Action<Exception> onError)
        {
            Enqueue(() =>
            {
                try
                {
                    using (var svc = new CrmServiceClient())
                        svc.DeleteClient(clientId);

                    onSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _disposed = true;

                // будим воркер чтобы он увидел _disposed и завершился
                Monitor.Pulse(_sync);
            }
        }
    }
}