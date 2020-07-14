#if TASKS

using System;
using System.Threading.Tasks;

using Xunit;

namespace Stateless.Tests
{
    public class AsyncActionsFixture
    {
        [Fact]
        public void StateMutatorShouldBeCalledOnlyOnce()
        {
            var state = State.B;
            var count = 0;
            var sm = new StateMachine<State, Trigger>(() => state, (s) => { state = s; count++; });
            sm.Configure(State.B).Permit(Trigger.X, State.C);
            sm.FireAsync(Trigger.X);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CanFireAsyncEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.Configure(State.B)
              .OnEntryAsync(() => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
            Assert.Equal(State.B, sm.State); // Should transition to destination state
        }

        [Fact]
        public void WhenSyncFireAsyncEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
              .OnEntryAsync(() => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Fact]
        public async Task CanFireAsyncExitAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
              .OnExitAsync(() => Task.Run(() => test = "foo"))
              .Permit(Trigger.X, State.B);

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
            Assert.Equal(State.B, sm.State); // Should transition to destination state
        }

        [Fact]
        public void WhenSyncFireAsyncExitAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .OnExitAsync(() => TaskResult.Done)
              .Permit(Trigger.X, State.B);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Fact]
        public async Task CanFireInternalAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
              .InternalTransitionAsync(Trigger.X, () => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public void WhenSyncFireInternalAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .InternalTransitionAsync(Trigger.X, () => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Fact]
        public async Task CanInvokeOnTransitionedAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.OnTransitionedAsync(_ => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }

        [Fact]
        public async Task WillInvokeSyncOnTransitionedIfRegisteredAlongWithAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test1 = "";
            var test2 = "";
            sm.OnTransitioned(_ => test1 = "foo1");
            sm.OnTransitionedAsync(_ => Task.Run(() => test2 = "foo2"));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal("foo1", test1);
            Assert.Equal("foo2", test2);
        }

        [Fact]
        public void WhenSyncFireAsyncOnTransitionedAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.OnTransitionedAsync(_ => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Fact]
        public async Task CanInvokeOnUnhandledTriggerAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.OnUnhandledTriggerAsync((s, t, u) => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.Z).ConfigureAwait(false);

            Assert.Equal("foo", test); // Should await action
        }
        [Fact]
        public void WhenSyncFireOnUnhandledTriggerAsyncTask()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.OnUnhandledTriggerAsync((s, t) => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.Z));
        }
        [Fact]
        public void WhenSyncFireOnUnhandledTriggerAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.OnUnhandledTriggerAsync((s, t, u) => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.Z));
        }

        [Fact]
        public async Task WhenActivateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var activated = false;
            sm.Configure(State.A)
              .OnActivateAsync(() => Task.Run(() => activated = true));

            await sm.ActivateAsync().ConfigureAwait(false);

            Assert.Equal(true, activated); // Should await action
        }

        [Fact]
        public async Task WhenDeactivateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var deactivated = false;
            sm.Configure(State.A)
              .OnDeactivateAsync(() => Task.Run(() => deactivated = true));

            await sm.ActivateAsync().ConfigureAwait(false);
            await sm.DeactivateAsync().ConfigureAwait(false);

            Assert.Equal(true, deactivated); // Should await action
        }

        [Fact]
        public void WhenSyncActivateAsyncOnActivateAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .OnActivateAsync(() => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Activate());
        }

        [Fact]
        public void WhenSyncDeactivateAsyncOnDeactivateAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .OnDeactivateAsync(() => TaskResult.Done);

            sm.Activate();

            Assert.Throws<InvalidOperationException>(() => sm.Deactivate());
        }
        [Fact]
        public async void IfSelfTransitionPermited_ActionsFire_InSubstate_async()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            bool onEntryStateBfired = false;
            bool onExitStateBfired = false;
            bool onExitStateAfired = false;

            sm.Configure(State.B)
                .OnEntryAsync(t => Task.Run(() => onEntryStateBfired = true))
                .PermitReentry(Trigger.X)
                .OnExitAsync(t => Task.Run(() => onExitStateBfired = true));

            sm.Configure(State.A)
                .SubstateOf(State.B)
                .OnExitAsync(t => Task.Run(() => onExitStateAfired = true));

            await sm.FireAsync(Trigger.X).ConfigureAwait(false);

            Assert.Equal(State.B, sm.State);
            Assert.True(onExitStateAfired);
            Assert.True(onExitStateBfired);
            Assert.True(onEntryStateBfired);
        }

        [Fact]
        public async void TransitionToSuperstateDoesNotExitSuperstate()
        {
            StateMachine<State, Trigger> sm = new StateMachine<State, Trigger>(State.B);

            bool superExit = false;
            bool superEntry = false;
            bool subExit = false;

            sm.Configure(State.A)
                .OnEntryAsync(t => Task.Run(() => superEntry = true))
                .OnExitAsync(t => Task.Run(() => superExit = true));

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .Permit(Trigger.Y, State.A)
                .OnExitAsync(t => Task.Run(() => subExit = true));

            await sm.FireAsync(Trigger.Y);

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
                .Permit(Trigger.X, State.C);

            stateMachine.Configure(State.B)
                .SubstateOf(State.A)
                .Ignore(Trigger.X);

            try
            {
                // >>> The following statement should not throw a NullReferenceException
                await stateMachine.FireAsync(Trigger.X);
            }
            catch (NullReferenceException )
            {
                nullRefExcThrown = true;
            }

            Assert.False(nullRefExcThrown);
        }

        [Fact]
        public void VerifyNotEnterSuperstateWhenDoingInitialTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C)
                .OnEntry(() => sm.Fire(Trigger.Y))
                .Permit(Trigger.Y, State.D);

            sm.Configure(State.C)
                .SubstateOf(State.B)
                .Permit(Trigger.Y, State.D);

            sm.FireAsync(Trigger.X);

            Assert.Equal(State.D, sm.State);
        }
    }
}

#endif
