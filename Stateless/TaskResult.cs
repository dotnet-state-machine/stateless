using System.Threading.Tasks;

namespace Stateless
{
    internal static class TaskResult
    {
        internal static readonly Task Done = FromResult(1);

        static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}