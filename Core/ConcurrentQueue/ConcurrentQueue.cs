using System.Collections.Generic;

namespace Core.ConcurrentQueue
{
    public class ConcurrentQueue<T> : IConcurrentQueue<T>
    {
        private readonly Queue<T> _queue;

        public ConcurrentQueue()
        {
            _queue = new Queue<T>();
        }

        public void AddTask(T action)
        {
            lock (_queue)
            {
                _queue.Enqueue(action);
            }
        }

        public bool TryTake(out T item)
        {
            lock (_queue)
            {
                if (_queue.Count != 0)
                {
                    item = _queue.Dequeue();
                    return true;
                }

                item = default;
                return false;
            }
        }

        public int Count()
        {
            return _queue.Count;
        }
    }
}