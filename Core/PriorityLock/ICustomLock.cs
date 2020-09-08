namespace Core.PriorityLock
{
    public interface ICustomLock
    {
        void Wait(int priority);
        void PulseAll();
    }
}