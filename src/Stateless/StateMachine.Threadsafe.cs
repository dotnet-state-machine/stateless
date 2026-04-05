using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger> 
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        /// <summary>
        /// Fires a trigger in a thread safe mode, so that if there are multiple triggers
        /// that have the possibility of being fired at the same time,and the state machine
        /// will still ingest them in some order and continue to function normally
        /// </summary>
        /// <param name="trigger">The trigger that shall be fired in a threadsafe manner. </param>
        public async Task FireThreadSafeAsync(TTrigger trigger)
        {
            await _semaphore.WaitAsync();
            try
            {
                await FireAsync(trigger);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Fires a trigger in a thread safe mode, so that if there are multiple triggers
        /// that have the possibility of being fired at the same time,and the state machine
        /// will still ingest them in some order and continue to function normally
        /// </summary>
        /// <param name="trigger">The trigger that shall be fired in a threadsafe manner. </param>
        /// <param name="args"> Parameters associated with the trigger. </param>
        public async Task FireThreadSafeAsync(TriggerWithParameters trigger, params object[] args)
        {
            await _semaphore.WaitAsync();
            try
            {
                await FireAsync(trigger, args);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// Extension of the State the current TState of the StateMachine in a thread-safe manner
        /// </summary>
        /// <returns>the current state of the StateMachine</returns>
        public async Task<TState> GetCurrentStateThreadSafeAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return State;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}