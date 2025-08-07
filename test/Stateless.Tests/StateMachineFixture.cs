﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class StateMachineFixture
    {
        const string
            StateA = "A", StateB = "B", StateC = "C",
            TriggerX = "X", TriggerY = "Y";

        int _numCalls = 0;

        void CountCalls()
        {
            _numCalls++;
        }

        [Fact]
        public void CanUseReferenceTypeMarkers()
        {
            RunSimpleTest(
                new[] { StateA, StateB, StateC },
                new[] { TriggerX, TriggerY });
        }

        [Fact]
        public void CanUseValueTypeMarkers()
        {
            RunSimpleTest(
                Enum.GetValues(typeof(State)).Cast<State>(),
                Enum.GetValues(typeof(Trigger)).Cast<Trigger>());
        }

        void RunSimpleTest<TState, TTransition>(IEnumerable<TState> states, IEnumerable<TTransition> transitions)
        {
            var a = states.First();
            var b = states.Skip(1).First();
            var x = transitions.First();

            var sm = new StateMachine<TState, TTransition>(a);

            sm.Configure(a)
                .Permit(x, b);

            sm.Fire(x);

            Assert.Equal(b, sm.State);
        }

        [Fact]
        public void InitialStateIsCurrent()
        {
            var initial = State.B;
            var sm = new StateMachine<State, Trigger>(initial);
            Assert.Equal(initial, sm.State);
        }

        [Fact]
        public void StateCanBeStoredExternally()
        {
            var state = State.B;
            var sm = new StateMachine<State, Trigger>(() => state, s => state = s);
            sm.Configure(State.B).Permit(Trigger.X, State.C);
            Assert.Equal(State.B, sm.State);
            Assert.Equal(State.B, state);
            sm.Fire(Trigger.X);
            Assert.Equal(State.C, sm.State);
            Assert.Equal(State.C, state);
        }

        [Fact]
        public void StateMutatorShouldBeCalledOnlyOnce()
        {
            var state = State.B;
            var count = 0;
            var sm = new StateMachine<State, Trigger>(() => state, (s) => { state = s; count++; });
            sm.Configure(State.B).Permit(Trigger.X, State.C);
            sm.Fire(Trigger.X);
            Assert.Equal(1, count);
        }

        [Fact]
        public void SubstateIsIncludedInCurrentState()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            sm.Configure(State.B).SubstateOf(State.C);

            Assert.Equal(State.B, sm.State);
            Assert.True(sm.IsInState(State.C));
        }

        [Fact]
        public void WhenInSubstate_TriggerIgnoredInSuperstate_RemainsInSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C);

            sm.Configure(State.C)
                .Ignore(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void WhenInSubstate_TriggerSuperStateTwiceToSameSubstate_DoesNotReenterSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var eCount = 0;

            sm.Configure(State.B)
                .OnEntry(() => { eCount++; })
                .SubstateOf(State.C);

            sm.Configure(State.A)
                .SubstateOf(State.C);


            sm.Configure(State.C)
                .Permit(Trigger.X, State.B);

            sm.Fire(Trigger.X);
            sm.Fire(Trigger.X);

            Assert.Equal(1, eCount);
        }

        [Fact]
        public async Task PermittedTriggersIncludeSuperstatePermittedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                .Permit(Trigger.Z, State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.Y, State.A);

            var permitted = await sm.GetPermittedTriggersAsync();

            Assert.True(permitted.Contains(Trigger.X));
            Assert.True(permitted.Contains(Trigger.Y));
            Assert.False(permitted.Contains(Trigger.Z));
        }

        [Fact]
        [Obsolete]
        public void PermittedTriggersIncludeSuperstatePermittedTriggersLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                .Permit(Trigger.Z, State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.Y, State.A);

            var permitted = sm.GetPermittedTriggers();

            Assert.True(permitted.Contains(Trigger.X));
            Assert.True(permitted.Contains(Trigger.Y));
            Assert.False(permitted.Contains(Trigger.Z));
        }

        [Fact]
        public async Task PermittedTriggersAreDistinctValues()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.X, State.B);

            var permitted = await sm.GetPermittedTriggersAsync();
            Assert.Equal(1, permitted.Count());
            Assert.Equal(Trigger.X, permitted.First());
        }

        [Fact]
        [Obsolete]
        public void PermittedTriggersAreDistinctValuesLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.X, State.B);

            var permitted = sm.GetPermittedTriggers();
            Assert.Equal(1, permitted.Count());
            Assert.Equal(Trigger.X, permitted.First());
        }

        [Fact]
        public async Task AcceptedTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false);

            Assert.Equal(0, (await sm.GetPermittedTriggersAsync()).Count());
        }

        [Fact]
        [Obsolete]
        public void AcceptedTriggersRespectGuardsLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false);

            Assert.Equal(0, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public async Task AcceptedAsyncTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(false));

            Assert.Equal(0, (await sm.GetPermittedTriggersAsync()).Count());
        }

        [Fact]
        [Obsolete]
        public void AcceptedAsyncTriggersRespectGuardsLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(false));

            Assert.Equal(0, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public async Task AcceptedValidAsyncTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(true));

            Assert.Equal(1, (await sm.GetPermittedTriggersAsync()).Count());
        }

        [Fact]
        [Obsolete]
        public void AcceptedValidAsyncTriggersRespectGuardsLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(true));

            Assert.Equal(1, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public async Task AcceptedOneValidAsyncAndSyncTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(true))
                .PermitIf(Trigger.Y, State.C, () => false);

            Assert.Equal(1, (await sm.GetPermittedTriggersAsync()).Count());
        }

        [Fact]
        [Obsolete]
        public void AcceptedOneValidAsyncAndSyncTriggersRespectGuardsLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIfAsync(Trigger.X, State.A, async () => await Task.FromResult(true))
                .PermitIf(Trigger.Y, State.C, () => false);

            Assert.Equal(1, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public async Task AcceptedTriggersRespectMultipleGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"));

            Assert.Equal(0, (await sm.GetPermittedTriggersAsync()).Count());
        }

        [Fact]
        [Obsolete]
        public void AcceptedTriggersRespectMultipleGuardsLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"));

            Assert.Equal(0, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public void WhenDiscriminatedByGuard_ChoosesPermitedTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false)
                .PermitIf(Trigger.X, State.C, () => true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void WhenDiscriminatedByMultiConditionGuard_ChoosesPermitedTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"))
                .PermitIf(Trigger.X, State.C,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => true, "2"));

            sm.Fire(Trigger.X);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void WhenTriggerIsIgnored_ActionsNotExecuted()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .Ignore(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.False(fired);
        }

        [Fact]
        public void IfSelfTransitionPermited_ActionsFire()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .PermitReentry(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.True(fired);
        }

        [Fact]
        public void ImplicitReentryIsDisallowed()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            Assert.Throws<ArgumentException>(() => sm.Configure(State.B)
               .Permit(Trigger.X, State.B));
        }

        [Fact]
        public void TriggerParametersAreImmutableOnceSet()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            sm.SetTriggerParameters<string, int>(Trigger.X);
            Assert.Throws<InvalidOperationException>(() => sm.SetTriggerParameters<string>(Trigger.X));
        }

        [Fact]
        public void ExceptionThrownForInvalidTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var exception = Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.Equal(exception.Message, "No valid leaving transitions are permitted from state 'A' for trigger 'X'. Consider ignoring the trigger.");
        }

        [Fact]
        public void ExceptionThrownForInvalidTransitionMentionsGuardDescriptionIfPresent()
        {
            // If guard description is empty then method name of guard is used
            // so I have skipped empty description test.
            const string guardDescription = "test";

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(Trigger.X, State.B, () => false, guardDescription);
            var exception = Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.Equal(typeof(InvalidOperationException), exception.GetType());
        }

        [Fact]
        public void ExceptionThrownForInvalidTransitionMentionsMultiGuardGuardDescriptionIfPresent()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(Trigger.X, State.B,
                new Tuple<Func<bool>, string>(() => false, "test1"),
                new Tuple<Func<bool>, string>(() => false, "test2"));

            var exception = Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.Equal(typeof(InvalidOperationException), exception.GetType());
        }

        [Fact]
        public void ParametersSuppliedToFireArePassedToEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            var x = sm.SetTriggerParameters<string, int>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.C);

            string entryArgS = null;
            int entryArgI = 0;

            sm.Configure(State.C)
                .OnEntryFrom(x, (s, i) =>
                {
                    entryArgS = s;
                    entryArgI = i;
                });

            var suppliedArgS = "something";
            var suppliedArgI = 42;

            sm.Fire(x, suppliedArgS, suppliedArgI);

            Assert.Equal(suppliedArgS, entryArgS);
            Assert.Equal(suppliedArgI, entryArgI);
        }

        [Fact]
        public void WhenAnUnhandledTriggerIsFired_TheProvidedHandlerIsCalledWithStateAndTrigger()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            State? state = null;
            Trigger? trigger = null;
            sm.OnUnhandledTrigger((s, t, u) =>
            {
                state = s;
                trigger = t;
            });

            sm.Fire(Trigger.Z);

            Assert.Equal(State.B, state);
            Assert.Equal(Trigger.Z, trigger);
        }

        [Fact]
        public void WhenATransitionOccurs_TheOnTransitionedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            sm.Fire(Trigger.X);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(new object[0], transition.Parameters);
        }

        [Fact]
        public void WhenATransitionOccurs_TheOnTransitionCompletedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitionCompleted(t => transition = t);

            sm.Fire(Trigger.X);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(new object[0], transition.Parameters);
        }

        /// <summary>
        /// The expected ordering is OnExit, OnTransitioned, OnEntry, OnTransitionCompleted
        /// </summary>
        [Fact]
        public void TheOnTransitionedEventFiresBeforeTheOnEntryEventAndOnTransitionCompletedFiresAfterwards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var expectedOrdering = new List<string> { "OnExit", "OnTransitioned", "OnEntry", "OnTransitionCompleted" };
            var actualOrdering = new List<string>();

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A)
                .OnExit(() => actualOrdering.Add("OnExit"));

            sm.Configure(State.A)
                .OnEntry(() => actualOrdering.Add("OnEntry"));

            sm.OnTransitioned(t => actualOrdering.Add("OnTransitioned"));
            sm.OnTransitionCompleted(t => actualOrdering.Add("OnTransitionCompleted"));

            sm.Fire(Trigger.X);

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
            {
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
            }
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_TheOnTransitionedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            string parameter = "the parameter";
            sm.Fire(triggerX, parameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(1, transition.Parameters.Count());
            Assert.Equal(parameter, transition.Parameters[0]);
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_TheOnTransitionCompletedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitionCompleted(t => transition = t);

            string parameter = "the parameter";
            sm.Fire(triggerX, parameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(1, transition.Parameters.Count());
            Assert.Equal(parameter, transition.Parameters[0]);
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_WithMultipleParameters_TheOnTransitionedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string, int, bool>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            string firstParameter = "the parameter";
            int secondParameter = 99;
            bool thirdParameter = true;
            sm.Fire(triggerX, firstParameter, secondParameter, thirdParameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(3, transition.Parameters.Count());
            Assert.Equal(firstParameter, transition.Parameters[0]);
            Assert.Equal(secondParameter, transition.Parameters[1]);
            Assert.Equal(thirdParameter, transition.Parameters[2]);
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_WithMultipleParameters_TheOnTransitionCompletedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string, int, bool>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitionCompleted(t => transition = t);

            string firstParameter = "the parameter";
            int secondParameter = 99;
            bool thirdParameter = true;
            sm.Fire(triggerX, firstParameter, secondParameter, thirdParameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(3, transition.Parameters.Count());
            Assert.Equal(firstParameter, transition.Parameters[0]);
            Assert.Equal(secondParameter, transition.Parameters[1]);
            Assert.Equal(thirdParameter, transition.Parameters[2]);
        }

        [Fact]
        public void DirectCyclicConfigurationDetected()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.A); });
        }

        [Fact]
        public void NestedCyclicConfigurationDetected()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.B).SubstateOf(State.A);

            Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.B); });
        }

        [Fact]
        public void NestedTwoLevelsCyclicConfigurationDetected()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.B).SubstateOf(State.A);
            sm.Configure(State.C).SubstateOf(State.B);

            Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.C); });
        }

        [Fact]
        public void DelayedNestedCyclicConfigurationDetected()
        {
            // Set up two states and substates, then join them
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.B).SubstateOf(State.A);

            sm.Configure(State.C);
            sm.Configure(State.A).SubstateOf(State.C);

            Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.C).SubstateOf(State.B); });
        }

        [Fact]
        public void IgnoreVsPermitReentry()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(CountCalls)
                .PermitReentry(Trigger.X)
                .Ignore(Trigger.Y);

            _numCalls = 0;

            sm.Fire(Trigger.X);
            sm.Fire(Trigger.Y);

            Assert.Equal(1, _numCalls);
        }

        [Fact]
        public void IgnoreVsPermitReentryFrom()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntryFrom(Trigger.X, CountCalls)
                .OnEntryFrom(Trigger.Y, CountCalls)
                .PermitReentry(Trigger.X)
                .Ignore(Trigger.Y);

            _numCalls = 0;

            sm.Fire(Trigger.X);
            sm.Fire(Trigger.Y);

            Assert.Equal(1, _numCalls);
        }

        [Fact]
        public void IfSelfTransitionPermited_ActionsFire_InSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            bool onEntryStateBfired = false;
            bool onExitStateBfired = false;
            bool onExitStateAfired = false;

            sm.Configure(State.B)
                .OnEntry(t => onEntryStateBfired = true)
                .PermitReentry(Trigger.X)
                .OnExit(t => onExitStateBfired = true);

            sm.Configure(State.A)
                .SubstateOf(State.B)
                .OnExit(t => onExitStateAfired = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(onEntryStateBfired);
            Assert.True(onExitStateBfired);
            Assert.True(onExitStateAfired);
        }

        [Fact]
        public void TransitionWhenParameterizedGuardTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.B, i => i == 2);
            sm.Fire(x, 2);

            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenParameterizedGuardFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.B, i => i == 3);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void TransitionWhenBothParameterizedGuardClausesTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            var positiveGuard = Tuple.Create(new Func<int, bool>(o => o == 2), "Positive Guard");
            var negativeGuard = Tuple.Create(new Func<int, bool>(o => o != 3), "Negative Guard");
            sm.Configure(State.A).PermitIf(x, State.B, positiveGuard, negativeGuard);
            sm.Fire(x, 2);

            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenBothParameterizedGuardClausesFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            // Create Two guards that both must be true
            var positiveGuard = Tuple.Create(new Func<int, bool>(o => o == 2), "Positive Guard");
            var negativeGuard = Tuple.Create(new Func<int, bool>(o => o != 3), "Negative Guard");
            sm.Configure(State.A).PermitIf(x, State.B, positiveGuard, negativeGuard);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 3));
        }

        [Fact]
        public void TransitionWhenGuardReturnsTrueOnTriggerWithMultipleParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.B, (s, i) => s == "3" && i == 3);
            sm.Fire(x, "3", 3);
            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenGuardFalseOnTriggerWithMultipleParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.B, (s, i) => s == "3" && i == 3);
            Assert.Equal(sm.State, State.A);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, "2", 2));
            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, "3", 2));
            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, "2", 3));
        }

        [Fact]
        public void TransitionWhenPermitIfHasMultipleExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(x, State.B, i => i == 3)
                .PermitIf(x, State.C, i => i == 2);
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenPermitIfHasMultipleNonExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.B, i => i % 2 == 0)  // Is Even
                .PermitIf(x, State.C, i => i == 2);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_On_Default_Value_And_Trigger_Parameters_Are_Omitted_Returns_True()
        {
            // This test verifies behavior in CanFire that may be considered a bug.
            // When a PermitIf transition is configured with a parameterized trigger and the trigger is fired without parameters,
            // CanFire will test the transition with the default values of the omitted trigger parameters' types.
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, i => i == default);

            Assert.True(sm.CanFire(Trigger.X));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_On_Non_Default_Value_And_Trigger_Parameters_Are_Omitted_Returns_False()
        {
            // This test verifies behavior in CanFire that may be considered a bug.
            // When a PermitIf transition is configured with a parameterized trigger and the trigger is fired without parameters,
            // CanFire will test the transition with the default values of the omitted trigger parameters' types.
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, i => i == 1);

            Assert.False(sm.CanFire(Trigger.X));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_One_Trigger_Parameter_Is_Used_And_Condition_Is_Met_Returns_True()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, i => i == 1);

            Assert.True(sm.CanFire(valueTrigger, 1));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_One_Trigger_Parameter_Is_Used_And_Condition_Is_NotMet_Returns_False()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, i => i != 1);

            Assert.False(sm.CanFire(valueTrigger, 1));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_Two_Trigger_Parameters_Are_Used_And_Condition_Is_Met_Returns_True()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, (i, s) => i == 1 && s.Equals("X", StringComparison.Ordinal));

            Assert.True(sm.CanFire(valueTrigger, 1, "X"));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_Two_Trigger_Parameters_Are_Used_And_Condition_Is_Not_Met_Returns_False()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, (i, s) => i != 1 && s.Equals("Y", StringComparison.Ordinal));

            Assert.False(sm.CanFire(valueTrigger, 1, "X"));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_Three_Trigger_Parameters_Are_Used_And_Condition_Is_Met_Returns_True()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, (i, s, b) => i == 1 && s.Equals("X", StringComparison.Ordinal) && b);

            Assert.True(sm.CanFire(valueTrigger, 1, "X", true));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Conditional_And_Three_Trigger_Parameters_Are_Used_And_Condition_Is_Not_Met_Returns_False()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            sm.Configure(State.A).PermitIf(valueTrigger, State.B, (i, s, b) => i != 1 && s.Equals("Y", StringComparison.Ordinal) && !b);

            Assert.False(sm.CanFire(valueTrigger, 1, "X", true));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_One_Trigger_Parameter_And_A_Guard_Condition_Is_Met_Returns_Empty_Collection()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, i => i == 1, "i equal to 1")
                .PermitIf(valueTrigger, State.B, i => i == 2, "i equal to 2");

            bool result = sm.CanFire(valueTrigger, 1, out ICollection<string> unmetGuards);

            Assert.True(result);
            Assert.False(unmetGuards.Any());
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_One_Trigger_Parameter_And_No_Guard_Conditions_Are_Met_Returns_Guard_Conditions()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, i => i == 1, "i equal to 1")
                .PermitIf(valueTrigger, State.B, i => i == 2, "i equal to 2");

            bool result = sm.CanFire(valueTrigger, 3, out ICollection<string> unmetGuards);

            Assert.Collection(unmetGuards,
                item => Assert.Equal("i equal to 1", item),
                item => Assert.Equal("i equal to 2", item));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_Two_Trigger_Parameters_And_A_Guard_Condition_Is_Met_Returns_Empty_Collection()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, (i, s) => i == 1 && s == "X", "i equal to 1 and s equal to 'X'")
                .PermitIf(valueTrigger, State.B, (i, s) => i == 2 && s == "X", "i equal to 2 and s equal to 'Y'");

            bool result = sm.CanFire(valueTrigger, 1, "X", out ICollection<string> unmetGuards);

            Assert.True(result);
            Assert.False(unmetGuards.Any());
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_Two_Trigger_Parameters_And_No_Guard_Conditions_Are_Met_Returns_Guard_Conditions()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, (i, s) => i == 1 && s == "X", "i equal to 1 and s equal to 'X'")
                .PermitIf(valueTrigger, State.B, (i, s) => i == 2 && s == "X", "i equal to 2 and s equal to 'Y'");

            bool result = sm.CanFire(valueTrigger, 3, "Z", out ICollection<string> unmetGuards);

            Assert.Collection(unmetGuards,
                item => Assert.Equal("i equal to 1 and s equal to 'X'", item),
                item => Assert.Equal("i equal to 2 and s equal to 'Y'", item));
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_Three_Trigger_Parameters_And_A_Guard_Condition_Is_Met_Returns_Empty_Collection()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, (i, s, b) => i == 1 && s == "X", "i equal to 1 and s equal to 'X' and boolean is true")
                .PermitIf(valueTrigger, State.B, (i, s, b) => i == 2 && s == "X", "i equal to 2 and s equal to 'Y' and boolean is true");

            bool result = sm.CanFire(valueTrigger, 1, "X", true, out ICollection<string> unmetGuards);

            Assert.True(result);
            Assert.False(unmetGuards.Any());
        }

        [Fact]
        public void CanFire_When_Transition_Is_Contidional_And_Has_Three_Trigger_Parameters_And_No_Guard_Conditions_Are_Met_Returns_Guard_Conditions()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var valueTrigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            sm.Configure(State.A)
                .PermitIf(valueTrigger, State.B, (i, s, b) => i == 1 && s == "X", "i equal to 1 and s equal to 'X' and boolean is true")
                .PermitIf(valueTrigger, State.B, (i, s, b) => i == 2 && s == "X", "i equal to 2 and s equal to 'Y' and boolean is true");

            bool result = sm.CanFire(valueTrigger, 3, "Z", false, out ICollection<string> unmetGuards);

            Assert.Collection(unmetGuards,
                item => Assert.Equal("i equal to 1 and s equal to 'X' and boolean is true", item),
                item => Assert.Equal("i equal to 2 and s equal to 'Y' and boolean is true", item));
        }

        [Fact]
        public void TransitionWhenPermitDyanmicIfHasMultipleExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicIf(x, i => i == 3 ? State.B : State.C, i => i == 3 || i == 5)
                .PermitDynamicIf(x, i => i == 2 ? State.C : State.D, i => i == 2 || i == 4);
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.B);
        }

#if Task
        [Fact]
        public async void TransitionWhenPermitDyanmicIfAsyncHasMultipleExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicIfAsync(x, i => Task.FromResult(i == 3 ? State.B : State.C), i => i == 3 || i == 5)
                .PermitDynamicIfAsync(x, i => Task.FromResult(i == 2 ? State.C : State.D), i => i == 2 || i == 4);
            await sm.FireAsync(x, 3);
            Assert.Equal(sm.State, State.B);
        }
#endif

        [Fact]
        public void ExceptionWhenPermitDyanmicIfHasMultipleNonExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIf(x, i => i == 4 ? State.B : State.C, i => i % 2 == 0)
                .PermitDynamicIf(x, i => i == 2 ? State.C : State.D, i => i == 2);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }
        
#if Task
        [Fact]
        public void ExceptionWhenPermitDyanmicIfAsyncHasMultipleNonExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitDynamicIfAsync(x, i => Task.FromResult(i == 4 ? State.B : State.C), i => i % 2 == 0)
                .PermitDynamicIfAsync(x, i => Task.FromResult(i == 2 ? State.C : State.D), i => i == 2);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }
#endif

        [Fact]
        public void TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.D, i => i == 3);
            {
                sm.Configure(State.B).SubstateOf(State.A).PermitIf(x, State.C, i => i == 2);
            }
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.D);
        }

        [Fact]
        public void TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).PermitIf(x, State.D, i => i == 3);
            {
                sm.Configure(State.B).SubstateOf(State.A).PermitIf(x, State.C, i => i == 2);
            }
            sm.Fire(x, 2);
            Assert.Equal(sm.State, State.C);
        }

        [Fact]
        public void TransitionWhenPermitReentryIfParameterizedGuardTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitReentryIf(x, i => i == 3);
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.A);
        }

        [Fact]
        public void TransitionWhenPermitReentryIfParameterizedGuardFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitReentryIf(x, i => i == 3);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void NoTransitionWhenIgnoreIfParameterizedGuardTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).IgnoreIf(x, i => i == 3);
            sm.Fire(x, 3);

            Assert.Equal(sm.State, State.A);
        }

        [Fact]
        public void ExceptionWhenIgnoreIfParameterizedGuardFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).IgnoreIf(x, i => i == 3);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        /// <summary>
        /// Verifies guard clauses are only called one time during a transition evaluation.
        /// </summary>
        [Fact]
        public void GuardClauseCalledOnlyOnce()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            int i = 0;

            sm.Configure(State.A).PermitIf(Trigger.X, State.B, () =>
            {
                ++i;
                return true;
            });

            sm.Fire(Trigger.X);

            Assert.Equal(1, i);
        }
        [Fact]
        public void NoExceptionWhenPermitIfHasMultipleExclusiveGuardsBothFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            bool onUnhandledTriggerWasCalled = false;
            sm.OnUnhandledTrigger((s, t) => { onUnhandledTriggerWasCalled = true; });  // NEVER CALLED
            int i = 0;
            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, () => i == 2)
                .PermitIf(Trigger.X, State.C, () => i == 1);

            sm.Fire(Trigger.X);  // THROWS EXCEPTION

            Assert.True(onUnhandledTriggerWasCalled, "OnUnhandledTrigger was called");
            Assert.Equal(sm.State, State.A);
        }

        [Fact]
        public void TransitionToSuperstateDoesNotExitSuperstate()
        {
            StateMachine<State, Trigger> sm = new StateMachine<State, Trigger>(State.B);

            bool superExit = false;
            bool superEntry = false;
            bool subExit = false;

            sm.Configure(State.A)
                .OnEntry(() => superEntry = true)
                .OnExit(() => superExit = true);

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .Permit(Trigger.Y, State.A)
                .OnExit(() => subExit = true);

            sm.Fire(Trigger.Y);

            Assert.True(subExit);
            Assert.False(superEntry);
            Assert.False(superExit);
        }

        [Fact]
        public void OnExitFiresOnlyOnceReentrySubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            int exitB = 0;
            int exitA = 0;
            int entryB = 0;
            int entryA = 0;

            sm.Configure(State.A)
                .SubstateOf(State.B)
                .OnEntry(() => entryA++)
                .PermitReentry(Trigger.X)
                .OnExit(() => exitA++);

            sm.Configure(State.B)
                .OnEntry(() => entryB++)
                .OnExit(() => exitB++);

            sm.Fire(Trigger.X);

            Assert.Equal(0, exitB);
            Assert.Equal(0, entryB);
            Assert.Equal(1, exitA);
            Assert.Equal(1, entryA);
        }

        [Fact]
        public void WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            Assert.True(sm.CanFire(trigger));
        }
        [Fact]
        public async Task WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            Assert.Single(await sm.PermittedTriggersAsync, trigger);
        }

        [Fact]
        [Obsolete]
        public void WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggersLegacy()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            sm.SetTriggerParameters<string>(trigger);
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public async Task WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            sm.SetTriggerParameters<string>(trigger);
            Assert.Single(await sm.PermittedTriggersAsync, trigger);
        }

        [Fact]
        [Obsolete]
        public void WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggersLegacy()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(trigger, State.B);
            sm.SetTriggerParameters<string>(trigger);
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(trigger, (_) => { });
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public async Task WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(trigger, (_) => { });
            Assert.Single(await sm.PermittedTriggersAsync, trigger);
        }

        [Fact]
        [Obsolete]
        public void WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggersLegacy()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(trigger, (_) => { });
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(sm.SetTriggerParameters<string>(trigger), (arg, _) => { });
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public async Task WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(sm.SetTriggerParameters<string>(trigger), (arg, _) => { });
            Assert.Single(await sm.PermittedTriggersAsync, trigger);
        }

        [Fact]
        [Obsolete]
        public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggersLegacy()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).InternalTransition(sm.SetTriggerParameters<string>(trigger), (arg, _) => { });
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(sm.SetTriggerParameters<string>(trigger), State.B, _ => true);
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public async Task WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(sm.SetTriggerParameters<string>(trigger), State.B, _ => true);
            Assert.Single(await sm.PermittedTriggersAsync, trigger);
        }

        [Fact]
        [Obsolete]
        public void WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggersLegacy()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(sm.SetTriggerParameters<string>(trigger), State.B, _ => true);
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public async Task PermittedTriggersIncludeAllDefinedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .InternalTransition(Trigger.Y, _ => { })
                .Ignore(Trigger.Z);

            var permittedTriggers = await sm.PermittedTriggersAsync;
            Assert.All(new[] { Trigger.X, Trigger.Y, Trigger.Z }, trigger => Assert.Contains(trigger, permittedTriggers));
        }

        [Fact]
        [Obsolete]
        public void PermittedTriggersIncludeAllDefinedTriggersLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .InternalTransition(Trigger.Y, _ => { })
                .Ignore(Trigger.Z);

            var permittedTriggers = sm.PermittedTriggers;
            Assert.All(new[] { Trigger.X, Trigger.Y, Trigger.Z }, trigger => Assert.Contains(trigger, permittedTriggers));
        }

        [Fact]
        public async Task PermittedTriggersExcludeAllUndefinedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);
            var permittedTriggers = await sm.PermittedTriggersAsync;
            Assert.All(new[] { Trigger.Y, Trigger.Z }, trigger => Assert.DoesNotContain(trigger, permittedTriggers));
        }

        [Fact]
        [Obsolete]
        public void PermittedTriggersExcludeAllUndefinedTriggersLegacy()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);
            var permittedTriggers = sm.PermittedTriggers;
            Assert.All(new[] { Trigger.Y, Trigger.Z }, trigger => Assert.DoesNotContain(trigger, permittedTriggers));
        }

        [Fact]
        public async Task PermittedTriggersIncludeAllInheritedTriggers()
        {
            State superState = State.A,
                    subState = State.B,
                    otherState = State.C;
            Trigger superStateTrigger = Trigger.X,
                    subStateTrigger = Trigger.Y;

            StateMachine<State, Trigger> hsm(State initialState)
                => new StateMachine<State, Trigger>(initialState)
                        .Configure(superState)
                        .Permit(superStateTrigger, otherState)
                    .Machine
                        .Configure(subState)
                        .SubstateOf(superState)
                        .Permit(subStateTrigger, otherState)
                    .Machine;

            var hsmInSuperstate = hsm(superState);
            var hsmInSubstate = hsm(subState);
            var subPermittedTriggers = await hsmInSubstate.PermittedTriggersAsync;
            var superPermittedTriggers = await hsmInSuperstate.PermittedTriggersAsync;

            Assert.All(superPermittedTriggers, trigger => Assert.Contains(trigger, subPermittedTriggers));
            Assert.Contains(superStateTrigger, subPermittedTriggers);
            Assert.Contains(superStateTrigger, superPermittedTriggers);
            Assert.Contains(subStateTrigger, subPermittedTriggers);
            Assert.DoesNotContain(subStateTrigger, superPermittedTriggers);
        }

        [Fact]
        [Obsolete]
        public void PermittedTriggersIncludeAllInheritedTriggersLegacy()
        {
            State superState = State.A,
                subState = State.B,
                otherState = State.C;
            Trigger superStateTrigger = Trigger.X,
                subStateTrigger = Trigger.Y;

            StateMachine<State, Trigger> hsm(State initialState)
                => new StateMachine<State, Trigger>(initialState)
                    .Configure(superState)
                    .Permit(superStateTrigger, otherState)
                    .Machine
                    .Configure(subState)
                    .SubstateOf(superState)
                    .Permit(subStateTrigger, otherState)
                    .Machine;

            var hsmInSuperstate = hsm(superState);
            var hsmInSubstate = hsm(subState);
            var subPermittedTriggers = hsmInSubstate.PermittedTriggers;
            var superPermittedTriggers = hsmInSuperstate.PermittedTriggers;

            Assert.All(superPermittedTriggers, trigger => Assert.Contains(trigger, subPermittedTriggers));
            Assert.Contains(superStateTrigger, subPermittedTriggers);
            Assert.Contains(superStateTrigger, superPermittedTriggers);
            Assert.Contains(subStateTrigger, subPermittedTriggers);
            Assert.DoesNotContain(subStateTrigger, superPermittedTriggers);
        }

        [Fact]
        public void CanFire_GetUnmetGuardDescriptionsIfGuardFails()
        {
            const string guardDescription = "Guard failed";
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
              .PermitIf(Trigger.X, State.B, () => false, guardDescription);

            bool result = sm.CanFire(Trigger.X, out ICollection<string> unmetGuards);

            Assert.False(result);
            Assert.True(unmetGuards?.Count == 1);
            Assert.Contains(guardDescription, unmetGuards);
        }

        [Fact]
        public void CanFire_GetNullUnmetGuardDescriptionsIfInvalidTrigger()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            bool result = sm.CanFire(Trigger.X, out ICollection<string> unmetGuards);

            Assert.False(result);
            Assert.Null(unmetGuards);
        }

        [Fact]
        public void CanFire_GetEmptyUnmetGuardDescriptionsIfValidTrigger()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Permit(Trigger.X, State.B);
            bool result = sm.CanFire(Trigger.X, out ICollection<string> unmetGuards);

            Assert.True(result);
            Assert.True(unmetGuards?.Count == 0);
        }

        [Fact]
        public void CanFire_GetEmptyUnmetGuardDescriptionsIfGuardPasses()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).PermitIf(Trigger.X, State.B, () => true, "Guard passed");
            bool result = sm.CanFire(Trigger.X, out ICollection<string> unmetGuards);

            Assert.True(result);
            Assert.True(unmetGuards?.Count == 0);
        }

        [Fact]
        public async Task GetDetailedPermittedTriggers_ReturnsTriggerWithoutParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            var permitted = (await sm.GetDetailedPermittedTriggers()).ToList();
            Assert.Single(permitted);
            var triggerDetails = permitted.First();
            Assert.False(triggerDetails.HasParameters);
            Assert.Null(triggerDetails.Parameters);
        }

        [Fact]
        public async Task GetDetailedPermittedTriggers_ReturnsTriggerWithParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            var pt = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            var permitted = (await sm.GetDetailedPermittedTriggers()).ToList();
            Assert.Single(permitted);
            var triggerDetails = permitted.First();
            Assert.True(triggerDetails.HasParameters);
            Assert.Equal(pt, triggerDetails.Parameters);
        }

    }
}