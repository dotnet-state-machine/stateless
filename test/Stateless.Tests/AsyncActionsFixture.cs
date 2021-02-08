using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Stateless.Tests
{
    public class AsyncActionsFixture
    {
        [Fact]
        public async Task StateMutatorShouldBeCalledOnlyOnce()
        {
            var state = State.B;
            var count = 0;
            var sm = new StateMachine<State, Trigger>(() => state, (s) => { state = s; count++; });
            sm.Configure(State.B).Transition(Trigger.X).To(State.C);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CanFireAsyncEntryAction()
        {
            var semaphore = new SemaphoreSlim(0);
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B);

            var test = "";
            sm.Configure(State.B)
              .OnEntry(() => Task.Run(() => { test = "foo"; semaphore.Release(); }));

            await sm.FireAsync(Trigger.X);
            
            Assert.True(semaphore.Wait(20));

            Assert.Equal("foo", test); // Should await action
            Assert.Equal(State.B, sm.State); // Should transition to destination state
        }

        [Fact]
        public async Task CanFireAsyncExitAction()
        {
            var toggle = new ManualResetEvent(false);
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
              .OnExit(() => Task.Run(() => { test = "foo"; toggle.Set(); }))
              .Transition(Trigger.X).To(State.B);

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);
            toggle.WaitOne(20);
            Assert.Equal("foo", test); // Should await action
            Assert.Equal(State.B, sm.State); // Should transition to destination state
        }

        [Fact]
        public async Task CanFireInternalAsyncAction()
        {
            var toggle = new ManualResetEvent(false);
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do(() => Task.Run(() => { test = "foo"; toggle.Set(); }));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);
            toggle.WaitOne(20);
            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public async Task CanInvokeOnTransitionedAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Transition(Trigger.X).To(State.B);

            var test = "";
            sm.OnTransitioned(_ => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public async Task CanInvokeOnTransitionCompletedAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Transition(Trigger.X).To(State.B);

            var test = "";
            sm.OnTransitionCompleted(_ => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public async Task WillInvokeSyncOnTransitionedIfRegisteredAlongWithAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Transition(Trigger.X).To(State.B);

            var test1 = "";
            var test2 = "";
            sm.OnTransitioned(_ => test1 = "foo1");
            sm.OnTransitioned(_ => Task.Run(() => test2 = "foo2"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo1", test1);
            Assert.Equal("foo2", test2);
        }

        [Fact]
        public async Task WillInvokeSyncOnTransitionCompletedIfRegisteredAlongWithAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Transition(Trigger.X).To(State.B);

            var test1 = "";
            var test2 = "";
            sm.OnTransitionCompleted(_ => test1 = "foo1");
            sm.OnTransitionCompleted(_ => Task.Run(() => test2 = "foo2"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo1", test1);
            Assert.Equal("foo2", test2);
        }

        [Fact]
        public async Task CanInvokeOnUnhandledTriggerAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Transition(Trigger.X).To(State.B);

            var test = "";
            sm.OnUnhandledTrigger((s, t, u) => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.Z).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public async Task WhenActivateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var activated = false;
            sm.Configure(State.A)
              .OnActivate(() => Task.Run(() => activated = true));

            await sm.ActivateAsync().ConfigureAwait(false);

            Assert.Equal(true, activated); // Should await action
        }

        [Fact]
        public async Task WhenDeactivateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var deactivated = false;
            sm.Configure(State.A)
              .OnDeactivate(() => Task.Run(() => deactivated = true));

            await sm.ActivateAsync().ConfigureAwait(false);
            await sm.DeactivateAsync().ConfigureAwait(false);

            Assert.Equal(true, deactivated); // Should await action
        }

        [Fact]
        public async Task IfSelfTransitionPermited_ActionsFire_InSubstate_async()
        {
            var semaphore = new SemaphoreSlim(0);
            
            var sm = new StateMachine<State, Trigger>(State.A);

            bool onEntryStateBfired = false;
            bool onExitStateBfired = false;
            bool onExitStateAfired = false;

            sm.Configure(State.B)
                .OnEntry(t => Task.Run(() => { onEntryStateBfired = true; semaphore.Release(); }))
                .Transition(Trigger.X).Self()
                .OnExit(t => Task.Run(() => { onExitStateBfired = true;  }));

            sm.Configure(State.A)
                .SubstateOf(State.B)
                .OnExit(t => Task.Run(() => { onExitStateAfired = true;  }));

            await sm.FireAsync(Trigger.X);
            Console.WriteLine("semaphore: " + semaphore.CurrentCount);
            semaphore.Wait(20);

            Assert.Equal(State.B, sm.State);
            Assert.True(onExitStateAfired);
            Assert.True(onExitStateBfired);
            Assert.True(onEntryStateBfired);
        }

        [Fact]
        public async Task TransitionToSuperstateDoesNotExitSuperstate()
        {
            var semaphore = new SemaphoreSlim(0);
            StateMachine<State, Trigger> sm = new StateMachine<State, Trigger>(State.B);

            bool superExit = false;
            bool superEntry = false;
            bool subExit = false;

            sm.Configure(State.A)
                .OnEntry(t => Task.Run(() => superEntry = true))
                .OnExit(t => Task.Run(() => superExit = true));

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .OnExit(t => Task.Run(() => { subExit = true; semaphore.Release(); }))
                .Transition(Trigger.Y).To(State.A);

            await sm.FireAsync(Trigger.Y);
            Assert.True(semaphore.Wait(20));
            Assert.True(subExit);
            Assert.False(superEntry);
            Assert.False(superExit);
        }

        [Fact]
        public async void IgnoredTriggerMustBeIgnoredAsync()
        {
            bool nullRefExcThrown = false;
            var stateMachine = new StateMachine<State, Trigger>(State.B);
            stateMachine.Configure(State.A)
                .Transition(Trigger.X).To(State.C);

            stateMachine.Configure(State.B)
                .SubstateOf(State.A)
                .Ignore(Trigger.X);

            try
            {
                // >>> The following statement should not throw a NullReferenceException
                await stateMachine.FireAsync(Trigger.X);
            }
            catch (NullReferenceException)
            {
                nullRefExcThrown = true;
            }

            Assert.False(nullRefExcThrown);
        }

        [Fact]
        public async void VerifyNotEnterSuperstateWhenDoingInitialTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C)
                .OnEntry(() => sm.Fire(Trigger.Y))
                .Transition(Trigger.Y).To(State.D);

            sm.Configure(State.C)
                .SubstateOf(State.B)
                .Transition(Trigger.Y).To(State.D);

            await sm.FireAsync(Trigger.X);

            Assert.Equal(State.D, sm.State);
        }
    }
}