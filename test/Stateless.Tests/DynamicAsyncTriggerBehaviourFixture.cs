using System;
using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class DynamicAsyncTriggerBehaviourFixture
    {
        [Fact]
        public async void PermitDynamic_Selects_Expected_State()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamicAsync(Trigger.X, () => Task.FromResult(State.B));

           await sm.FireAsync(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public async void PermitDynamic_With_TriggerParameter_Selects_Expected_State()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicAsync(trigger, i => Task.FromResult(i == 1 ? State.B : State.C));

            await sm.FireAsync(trigger, 1);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public async void PermitDynamic_Permits_Reentry()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var onExitInvoked = false;
            var onEntryInvoked = false;
            var onEntryFromInvoked = false;
            sm.Configure(State.A)
                .PermitDynamicAsync(Trigger.X, () => Task.FromResult(State.A))
                .OnEntry(() => onEntryInvoked = true)
                .OnEntryFrom(Trigger.X, () => onEntryFromInvoked = true)
                .OnExit(() => onExitInvoked = true);

           await sm.FireAsync(Trigger.X);

            Assert.True(onExitInvoked, "Expected OnExit to be invoked");
            Assert.True(onEntryInvoked, "Expected OnEntry to be invoked");
            Assert.True(onEntryFromInvoked, "Expected OnEntryFrom to be invoked");
            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public async void PermitDynamic_Selects_Expected_State_Based_On_DestinationStateSelector_Function()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var value = 'C';
            sm.Configure(State.A)
                .PermitDynamicAsync(Trigger.X, () => Task.FromResult(value == 'B' ? State.B : State.C));

          await sm.FireAsync(Trigger.X);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public async void PermitDynamicIf_With_TriggerParameter_Permits_Transition_When_GuardCondition_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicIfAsync(trigger, (i) => Task.FromResult(i == 1 ? State.C : State.B), (i) => i == 1);

           await sm.FireAsync(trigger, 1);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public async void PermitDynamicIf_With_2_TriggerParameters_Permits_Transition_When_GuardCondition_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIfAsync(
                trigger,
                (i, j) => Task.FromResult(i == 1 && j == 2 ? State.C : State.B),
                (i, j) => i == 1 && j == 2);

           await sm.FireAsync(trigger, 1, 2);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public async void PermitDynamicIf_With_3_TriggerParameters_Permits_Transition_When_GuardCondition_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int, int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIfAsync(
                trigger,
                (i, j, k) => Task.FromResult(i == 1 && j == 2 && k == 3 ? State.C : State.B),
                (i, j, k) => i == 1 && j == 2 && k == 3);

           await sm.FireAsync(trigger, 1, 2, 3);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void PermitDynamicIf_With_TriggerParameter_Throws_When_GuardCondition_Not_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicIfAsync(trigger, (i) => Task.FromResult(i > 0 ? State.C : State.B), (i) => i == 2 ? true : false);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(trigger, 1));
        }

        [Fact]
        public void PermitDynamicIf_With_2_TriggerParameters_Throws_When_GuardCondition_Not_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIfAsync(
                trigger,
                (i, j) => Task.FromResult(i > 0 ? State.C : State.B),
                (i, j) => i == 2 && j == 3);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(trigger, 1, 2));
        }

        [Fact]
        public void PermitDynamicIf_With_3_TriggerParameters_Throws_When_GuardCondition_Not_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int, int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIfAsync(trigger,
                (i, j, k) => Task.FromResult(i > 0 ? State.C : State.B),
                (i, j, k) => i == 2 && j == 3 && k == 4);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(trigger, 1, 2, 3));
        }

        [Fact]
        public async void PermitDynamicIf_Permits_Reentry_When_GuardCondition_Met()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var onExitInvoked = false;
            var onEntryInvoked = false;
            var onEntryFromInvoked = false;
            sm.Configure(State.A)
                .PermitDynamicIfAsync(Trigger.X, () => Task.FromResult(State.A), () => true)
                .OnEntry(() => onEntryInvoked = true)
                .OnEntryFrom(Trigger.X, () => onEntryFromInvoked = true)
                .OnExit(() => onExitInvoked = true);

            await sm.FireAsync(Trigger.X);

            Assert.True(onExitInvoked, "Expected OnExit to be invoked");
            Assert.True(onEntryInvoked, "Expected OnEntry to be invoked");
            Assert.True(onEntryFromInvoked, "Expected OnEntryFrom to be invoked");
            Assert.Equal(State.A, sm.State);
        }
    }
}
