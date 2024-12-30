using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Stateless.Tests
{
    public class SynchronizationContextFixture
    {
        // Define a custom SynchronizationContext. All calls made to delegates should be with this context.
        private readonly MaxConcurrencySyncContext _customSynchronizationContext = new MaxConcurrencySyncContext(3);
        private readonly List<SynchronizationContext> _capturedSyncContext = new List<SynchronizationContext>();
    
        private StateMachine<State, Trigger> GetSut(State initialState = State.A)
        {
            return new StateMachine<State, Trigger>(initialState, FiringMode.Queued)
            {
                RetainSynchronizationContext = true
            };
        }
    
        private void SetSyncContext()
        {
            SynchronizationContext.SetSynchronizationContext(_customSynchronizationContext);
        }

        /// <summary>
        /// Simulate a call that loses the synchronization context
        /// </summary>
        private async Task CaptureThenLoseSyncContext()
        {
            CaptureSyncContext();
            await LoseSyncContext().ConfigureAwait(false); // ConfigureAwait false here to ensure we continue using the sync context returned by LoseSyncContext 
        }
    
        private void CaptureSyncContext()
        {
            _capturedSyncContext.Add(SynchronizationContext.Current);
        }

        private async Task LoseSyncContext()
        {
            await new CompletesOnDifferentThreadAwaitable(); // Switch synchronization context and continue
            Assert.NotEqual(_customSynchronizationContext, SynchronizationContext.Current);
        }

        /// <summary>
        /// Tests capture the SynchronizationContext at various points throughout their execution.
        /// This asserts that every capture is the expected SynchronizationContext instance and that it hasn't been lost. 
        /// </summary>
        /// <param name="numberOfExpectedCalls">Ensure that we have the expected number of captures</param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void AssertSyncContextAlwaysRetained(int numberOfExpectedCalls)
        {
            Assert.Equal(numberOfExpectedCalls, _capturedSyncContext.Count);
            Assert.All(_capturedSyncContext, actual => Assert.Equal(_customSynchronizationContext, actual));
        }

        /// <summary>
        /// XUnit uses its own SynchronizationContext to execute each test. Therefore, placing SetSyncContext() in the constructor instead of
        /// at the start of every test does not work as desired. This test ensures XUnit's behaviour has not changed.
        /// </summary>
        [Fact]
        public void Ensure_XUnit_is_using_SyncContext()
        {
            SetSyncContext();
            CaptureSyncContext();
            AssertSyncContextAlwaysRetained(1);
        }
    
        /// <summary>
        /// SynchronizationContext are funny things. The way that they are lost varies depending on their implementation.
        /// This test ensures that our mechanism for losing the SynchronizationContext works.
        /// </summary>
        [Fact]
        public async Task Ensure_XUnit_can_lose_sync_context()
        {
            SetSyncContext();
            await LoseSyncContext().ConfigureAwait(false);
            Assert.NotEqual(_customSynchronizationContext, SynchronizationContext.Current);
        }

        [Fact]
        public async Task Activation_of_state_with_superstate_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .OnActivateAsync(CaptureThenLoseSyncContext)
                .SubstateOf(State.B);
            
            sm.Configure(State.B)
                .OnActivateAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.ActivateAsync();
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task Deactivation_of_state_with_superstate_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .OnDeactivateAsync(CaptureThenLoseSyncContext)
                .SubstateOf(State.B);
            
            sm.Configure(State.B)
                .OnDeactivateAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.DeactivateAsync();
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task Multiple_activations_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .OnActivateAsync(CaptureThenLoseSyncContext)
                .OnActivateAsync(CaptureThenLoseSyncContext)
                .OnActivateAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.ActivateAsync();
        
            // ASSERT
            AssertSyncContextAlwaysRetained(3);
        }
    
        [Fact]
        public async Task Multiple_Deactivations_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .OnDeactivateAsync(CaptureThenLoseSyncContext)
                .OnDeactivateAsync(CaptureThenLoseSyncContext)
                .OnDeactivateAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.DeactivateAsync();
        
            // ASSERT
            AssertSyncContextAlwaysRetained(3);
         }
    
        [Fact]
        public async Task Multiple_OnEntry_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A).Permit(Trigger.X, State.B);
            sm.Configure(State.B)
                .OnEntryAsync(CaptureThenLoseSyncContext)
                .OnEntryAsync(CaptureThenLoseSyncContext)
                .OnEntryAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(3);
        } 
    
        [Fact]
        public async Task Multiple_OnExit_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .OnExitAsync(CaptureThenLoseSyncContext)
                .OnExitAsync(CaptureThenLoseSyncContext)
                .OnExitAsync(CaptureThenLoseSyncContext);
            sm.Configure(State.B);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(3);
        }
    
        [Fact]
        public async Task OnExit_state_with_superstate_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut(State.B);
            sm.Configure(State.A)
                .OnExitAsync(CaptureThenLoseSyncContext)
                ;
        
            sm.Configure(State.B)
                .SubstateOf(State.A)
                .Permit(Trigger.X, State.C)
                .OnExitAsync(CaptureThenLoseSyncContext)
                ;
            sm.Configure(State.C);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task OnExit_state_and_superstate_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut(State.C);
            sm.Configure(State.A);

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .OnExitAsync(CaptureThenLoseSyncContext);
        
            sm.Configure(State.C)
                .SubstateOf(State.B)
                .Permit(Trigger.X, State.A)
                .OnExitAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task Multiple_OnEntry_on_Reentry_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A).PermitReentry(Trigger.X)
                .OnEntryAsync(CaptureThenLoseSyncContext)
                .OnEntryAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task Multiple_OnExit_on_Reentry_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A).PermitReentry(Trigger.X)
                .OnExitAsync(CaptureThenLoseSyncContext)
                .OnExitAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }

        [Fact]
        public async Task Trigger_firing_another_Trigger_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .InternalTransitionAsync(Trigger.X, async () =>
                {
                    await CaptureThenLoseSyncContext();
                    await sm.FireAsync(Trigger.Y);
                })
                .Permit(Trigger.Y, State.B)
                ;
            sm.Configure(State.B)
                .OnEntryAsync(CaptureThenLoseSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(2);
        }
    
        [Fact]
        public async Task OnTransition_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B);

            sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
            sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
            sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(3);
        }
    
        [Fact]
        public async Task InternalTransition_firing_a_sync_action_should_retain_SyncContext()
        {
            // ARRANGE
            SetSyncContext();
            var sm = GetSut();
            sm.Configure(State.A)
                .InternalTransition(Trigger.X, CaptureSyncContext);
        
            // ACT
            await sm.FireAsync(Trigger.X);
        
            // ASSERT
            AssertSyncContextAlwaysRetained(1);
        }
    
        private class CompletesOnDifferentThreadAwaitable
        {
            public CompletesOnDifferentThreadAwaiter GetAwaiter() => new CompletesOnDifferentThreadAwaiter();

            internal class CompletesOnDifferentThreadAwaiter : INotifyCompletion
            {
                public void GetResult() { }

                public bool IsCompleted => false;

                public void OnCompleted(Action continuation)
                {
                    ThreadPool.QueueUserWorkItem(_ => continuation());
                }
            }
        }
    }
}