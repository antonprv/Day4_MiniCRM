using System;
using System.Threading;

namespace MiniCRM.Client.Infrastructure
{
    public class Debounce : IDisposable
    {
        private Timer _timer;
        private readonly object _sync = new object();

        public void Run(int delayMs, Action action)
        {
            lock (_sync)
            {
                _timer?.Dispose();

                _timer = new Timer(
                    _ => action(),
                    null,
                    delayMs,
                    Timeout.Infinite
                    );
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _timer?.Dispose();

                _timer = null;
            }
        }
    }
}
