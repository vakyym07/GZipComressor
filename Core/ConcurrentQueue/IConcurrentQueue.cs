using System;

namespace Core.ConcurrentQueue
{
    public interface IConcurrentQueue<T>
    {
        void AddTask(T item);
        bool TryTake(out T item);
        int Count();
    }
}