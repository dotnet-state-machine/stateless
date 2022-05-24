namespace Stateless;

internal static class TaskResult {
    internal static readonly Task Done = FromResult(1);

    internal static Task FromException(Exception exception) {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetException(exception);
        return tcs.Task;
    }

    private static Task<T> FromResult<T>(T value) {
        var tcs = new TaskCompletionSource<T>();
        tcs.SetResult(value);
        return tcs.Task;
    }
}