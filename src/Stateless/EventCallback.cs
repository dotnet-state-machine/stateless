using System.Reflection;

namespace Stateless;

internal class EventCallback {
    protected readonly MulticastDelegate Delegate;

    public EventCallback(MulticastDelegate @delegate) => Delegate = @delegate;

    public Task InvokeAsync() {
        switch (Delegate) {
            case Action action:
                action();
                return TaskResult.Done;

            case Func<Task> func:
                return func();

            default:
                return DynamicInvokeAsync();
        }
    }

    protected Task DynamicInvokeAsync(params object?[] args) {
        switch (Delegate) {
            case null:
                return TaskResult.Done;

            default:
                try {
                    return Delegate.DynamicInvoke(args) as Task ?? TaskResult.Done;
                } catch (TargetInvocationException e) {
                    // Since we fell into the DynamicInvoke case, any exception will be wrapped
                    // in a TIE. We can expect this to be thrown synchronously, so it's low overhead
                    // to unwrap it.
                    return TaskResult.FromException(e.InnerException!);
                }
        }
    }
}

internal sealed class EventCallback<T> : EventCallback {
    public EventCallback(MulticastDelegate @delegate) : base(@delegate) { }

    public Task InvokeAsync(T arg) {
        switch (Delegate) {
            case Action<T> action:
                action(arg);
                return TaskResult.Done;

            case Func<T, Task> func:
                return func(arg);

            default:
                return DynamicInvokeAsync(arg);
        }
    }
}

internal sealed class EventCallback<T1, T2> : EventCallback {
    public EventCallback(MulticastDelegate @delegate) : base(@delegate) { }

    public Task InvokeAsync(T1 arg1, T2 arg2) {
        switch (Delegate) {
            case Action<T1, T2> action:
                action(arg1, arg2);
                return TaskResult.Done;

            case Func<T1, T2, Task> func:
                return func(arg1, arg2);

            default:
                return DynamicInvokeAsync(arg1, arg2);
        }
    }
}

internal sealed class EventCallback<T1, T2, T3> : EventCallback {
    public EventCallback(MulticastDelegate @delegate) : base(@delegate) { }

    public Task InvokeAsync(T1 arg1, T2 arg2, T3 arg3) {
        switch (Delegate) {
            case Action<T1, T2, T3> action:
                action(arg1, arg2, arg3);
                return TaskResult.Done;

            case Func<T1, T2, T3, Task> func:
                return func(arg1, arg2, arg3);

            default:
                return DynamicInvokeAsync(arg1, arg2, arg3);
        }
    }
}

internal sealed class EventCallback<T1, T2, T3, T4> : EventCallback {
    public EventCallback(MulticastDelegate @delegate) : base(@delegate) { }

    public Task InvokeAsync(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        switch (Delegate) {
            case Action<T1, T2, T3, T4> action:
                action(arg1, arg2, arg3, arg4);
                return TaskResult.Done;

            case Func<T1, T2, T3, T4, Task> func:
                return func(arg1, arg2, arg3, arg4);

            default:
                return DynamicInvokeAsync(arg1, arg2, arg3, arg4);
        }
    }
}