using System.Threading;

namespace Core.PriorityLock
{
    public class PriorityLock : ICustomLock
    {
        private readonly object _locker = new object();
        private int _currentWorkerPriority = 0;

        public void Wait(int priority)
        {
            lock (_locker)
            {
                while (priority != _currentWorkerPriority)
                {
                    Monitor.Wait(_locker);
                }
            }
        }

        public void PulseAll()
        {
            lock (_locker)
            {
                _currentWorkerPriority++;
                Monitor.PulseAll(_locker);
            }
        }
    }
}