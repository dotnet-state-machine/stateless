using System;
using System.Threading.Tasks;

namespace Stateless; 

internal static class EventCallbackFactory
{

    public static readonly EventCallback Empty = new(() => { });

    public static EventCallback Create(Action callback)
    {
        return new EventCallback(callback);
    }

    public static EventCallback Create(Func<Task> callback)
    {
        return new EventCallback(callback);
    }

    public static EventCallback<T> Create<T>(Action<T> callback)
    {
        return new EventCallback<T>(callback);
    }

    public static EventCallback<T> Create<T>(Func<T, Task> callback)
    {
        return new EventCallback<T>(callback);
    }

    public static EventCallback<T1, T2> Create<T1, T2>(Action<T1, T2> callback)
    {
        return new EventCallback<T1, T2>(callback);
    }

    public static EventCallback<T1, T2> Create<T1, T2>(Func<T1, T2, Task> callback)
    {
        return new EventCallback<T1, T2>(callback);
    }

    public static EventCallback<T1, T2, T3> Create<T1, T2, T3>(Action<T1, T2, T3> callback)
    {
        return new EventCallback<T1, T2, T3>(callback);
    }

    public static EventCallback<T1, T2, T3> Create<T1, T2, T3>(Func<T1, T2, T3, Task> callback)
    {
        return new EventCallback<T1, T2, T3>(callback);
    }

    public static EventCallback<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
    {
        return new EventCallback<T1, T2, T3, T4>(callback);
    }

    public static EventCallback<T1, T2, T3, T4> Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> callback)
    {
        return new EventCallback<T1, T2, T3, T4>(callback);
    }
}