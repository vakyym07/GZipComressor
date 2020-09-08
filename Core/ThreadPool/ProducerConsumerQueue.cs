using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.ConcurrentQueue;

namespace Core.ThreadPool
{
    public class ProducerConsumerQueue : IProducerConsumerQueue
    {
        private readonly IConcurrentQueue<Action> _concurrentQueue;
        private readonly CountdownEvent _countdownEvent;
        private readonly object _locker = new object();
        private readonly object _awaitLocker = new object();

        private readonly int _workerCount;
        private bool _isRunning;
        private readonly List<Exception> _errors;

        public ProducerConsumerQueue(int workerCount = 1)
        {
            _workerCount = workerCount;
            _isRunning = true;
            _concurrentQueue = new ConcurrentQueue<Action>();
            _errors = new List<Exception>();
            _countdownEvent = new CountdownEvent(workerCount);

            for (var i = 0; i < workerCount; i++)
            {
                new Thread(DoWork).Start();
            }
        }

        public bool TryExecute(Action action)
        {
            if (!_isRunning)
            {
                return false;
            }

            _concurrentQueue.AddTask(action);
            lock (_locker)
            {
                Monitor.PulseAll(_locker);
            }

            return true;
        }

        public void ShutDown()
        {
            if (!_isRunning) return;

            for (var i = 0; i < _workerCount; i++)
            {
                TryExecute(null);
            }

            _isRunning = false;
            lock (_locker)
            {
                Monitor.PulseAll(_locker);
            }

            _countdownEvent.Wait();
        }

        public void AwaitResult()
        {
            while (_concurrentQueue.Count() != 0 && _isRunning)
            {
                lock (_awaitLocker)
                {
                    Monitor.Wait(_awaitLocker);
                }
            }
        }

        public Exception[] Errors => _errors.ToArray();

        private void DoWork()
        {
            while (true)
            {
                if (_concurrentQueue.TryTake(out var action))
                {
                    if (!TryPerformAction(action))
                    {
                        break;
                    }
                }
                else
                {
                    lock (_awaitLocker)
                    {
                        Monitor.Pulse(_awaitLocker);
                    }

                    lock (_locker)
                    {
                        Monitor.Wait(_locker);
                    }
                }
            }

            _countdownEvent.Signal();
        }

        private bool TryPerformAction(Action action)
        {
            try
            {
                if (action == null)
                {
                    return false;
                }

                action();
            }
            catch (Exception e)
            {
                _errors.Add(e);
                return false;
            }

            return true;
        }
    }
}