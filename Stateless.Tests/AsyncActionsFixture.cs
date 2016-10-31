#if !NET40

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class AsyncActionsFixture
    {
        [Test]
        public async Task CanFireAsyncEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.Configure(State.B)
              .OnEntryAsync(() => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X);

            Assert.AreEqual("foo", test, "Should await action");
            Assert.AreEqual(State.B, sm.State, "Should transition to destination state");
        }

        [Test]
        public void WhenSyncFireAsyncEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
              .OnEntryAsync(() => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(()=> sm.Fire(Trigger.X));
        }

        [Test]
        public async Task CanFireAsyncExitAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
              .OnExitAsync(() => Task.Run(() => test = "foo"))
              .Permit(Trigger.X, State.B);

            await sm.FireAsync(Trigger.X);

            Assert.AreEqual("foo", test, "Should await action");
            Assert.AreEqual(State.B, sm.State, "Should transition to destination state");
        }

        [Test]
        public void WhenSyncFireAsyncExitAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .OnExitAsync(() => TaskResult.Done)
              .Permit(Trigger.X, State.B);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Test]
        public async Task CanFireInternalAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var test = "";
            sm.Configure(State.A)
              .InternalTransitionAsync(Trigger.X, ()=> Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X);

            Assert.AreEqual("foo", test, "Should await action");
        }

        [Test]
        public void WhenSyncFireInternalAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .InternalTransitionAsync(Trigger.X, ()=> TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Test]
        public async Task CanInvokeOnTransitionedAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.OnTransitionedAsync(_ => Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.X);

            Assert.AreEqual("foo", test, "Should await action");
        }

        [Test]
        public async Task WillInvokeSyncOnTransitionedIfRegisteredAlongWithAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test1 = "";
            var test2 = "";
            sm.OnTransitioned(_ => test1 = "foo1");
            sm.OnTransitionedAsync(_ => Task.Run(() => test2 = "foo2"));

            await sm.FireAsync(Trigger.X);

            Assert.AreEqual("foo1", test1);
            Assert.AreEqual("foo2", test2);
        }

        [Test]
        public void WhenSyncFireAsyncOnTransitionedAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.OnTransitionedAsync(_ => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
        }

        [Test]
        public async Task CanInvokeOnUnhandledTriggerAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            var test = "";
            sm.OnUnhandledTriggerAsync((s, t)=>Task.Run(() => test = "foo"));

            await sm.FireAsync(Trigger.Z);

            Assert.AreEqual("foo", test, "Should await action");
        }

        [Test]
        public void WhenSyncFireOnUnhandledTriggerAsyncAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
              .Permit(Trigger.X, State.B);

            sm.OnUnhandledTriggerAsync((s,t) => TaskResult.Done);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.Z));
        }
    }
}

#endif