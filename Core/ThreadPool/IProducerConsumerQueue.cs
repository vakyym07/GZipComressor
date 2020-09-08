using System;
using System.Collections.Generic;

namespace Core.ThreadPool
{
    public interface IProducerConsumerQueue
    {
        bool TryExecute(Action action);
        void ShutDown();
        void AwaitResult();

        Exception[] Errors { get; }
    }
}