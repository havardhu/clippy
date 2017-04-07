using System;
using System.Threading.Tasks;

namespace Clippy
{
    public class Throttler<T>
    {
        private readonly Action<T> _doAction;
        private readonly int _timeWaitMs;
        private int triggerCount = 0;
        private object triggerLock = new object();
        public Throttler(Action<T> doAction, int timeWaitMs = 100)
        {
            _doAction = doAction;
            _timeWaitMs = timeWaitMs;
        }

        public Task Trigger(T value)
        {
            int count;
            lock (triggerLock)
            {
                count = ++triggerCount;
            }

            return Task.Delay(_timeWaitMs).ContinueWith(t =>
            {
                if (count == triggerCount)
                {
                    _doAction(value);
                    triggerCount = 0;
                }
            });
        }
    }
}