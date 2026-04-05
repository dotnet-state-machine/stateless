using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger> : IDisposable
    {
        private bool _disposed = false;
        
        /// <summary>
        /// Disposes of the State Machine in-memory resources. View pattern for more details: <a href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">here</a>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Disposes of the State Machine in-memory resources. View pattern for more details: <a href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">here</a>
        /// </summary>
        /// <param name="disposing"> Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false). </param>
        protected void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Dispose();
                _disposed = true;
            }
            
            if (disposing)
            {
                // dispose of managed objects
                _semaphore?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.
            _disposed = true;
        }
    }
}