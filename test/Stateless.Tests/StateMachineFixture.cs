﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                .Transition(x).To(b);

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
            sm.Configure(State.B).Transition(Trigger.X).To(State.C);
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
            sm.Configure(State.B).Transition(Trigger.X).To(State.C);
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
        public void PermittedTriggersIncludeSuperstatePermittedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                .Transition(Trigger.Z).To(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Transition(Trigger.X).To(State.A);

            sm.Configure(State.C)
                .Transition(Trigger.Y).To(State.A);

            var permitted = sm.GetPermittedTriggers();

            Assert.True(permitted.Contains(Trigger.X));
            Assert.True(permitted.Contains(Trigger.Y));
            Assert.False(permitted.Contains(Trigger.Z));
        }

        [Fact]
        public void PermittedTriggersAreDistinctValues()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Transition(Trigger.X).To(State.A);

            sm.Configure(State.C)
                .Transition(Trigger.X).To(State.B);

            var permitted = sm.GetPermittedTriggers();
            Assert.Equal(1, permitted.Count());
            Assert.Equal(Trigger.X, permitted.First());
        }

        [Fact]
        public void AcceptedTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A).If(() => false);

            Assert.Equal(0, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public void AcceptedTriggersRespectMultipleGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A).If(
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"));

            Assert.Equal(0, sm.GetPermittedTriggers().Count());
        }

        [Fact]
        public void WhenDiscriminatedByGuard_ChoosesPermitedTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A).If(() => false)
                .Transition(Trigger.X).To(State.C).If(() => true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void WhenDiscriminatedByMultiConditionGuard_ChoosesPermitedTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A).If(
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"))
                .Transition(Trigger.X).To(State.C).If(
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
                .Transition(Trigger.X).Self();

            sm.Fire(Trigger.X);

            Assert.True(fired);
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
            sm.Configure(State.A).Transition(Trigger.X).To(State.B).If(() => false, guardDescription);
            var exception = Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.Equal(typeof(InvalidOperationException), exception.GetType());
        }

        [Fact]
        public void ExceptionThrownForInvalidTransitionMentionsMultiGuardGuardDescriptionIfPresent()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(Trigger.X).To(State.B).If(
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
                .Transition(Trigger.X).To(State.C);

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
                .Transition(Trigger.X).To(State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            sm.Fire(Trigger.X);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(Array.Empty<object>(), transition.Parameters);
        }

        [Fact]
        public void WhenATransitionOccurs_TheOnTransitionCompletedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitionCompleted(t => transition = t);

            sm.Fire(Trigger.X);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(Array.Empty<object>(), transition.Parameters);
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
                .Transition(Trigger.X).To(State.A)
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
                .Transition(Trigger.X).To(State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            string parameter = "the parameter";
            sm.Fire(triggerX, parameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(1, transition.Parameters.Length);
            Assert.Equal(parameter, transition.Parameters[0]);
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_TheOnTransitionCompletedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitionCompleted(t => transition = t);

            string parameter = "the parameter";
            sm.Fire(triggerX, parameter);

            Assert.NotNull(transition);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Source);
            Assert.Equal(State.A, transition.Destination);
            Assert.Equal(1, transition.Parameters.Length);
            Assert.Equal(parameter, transition.Parameters[0]);
        }

        [Fact]
        public void WhenATransitionOccurs_WithAParameterizedTrigger_WithMultipleParameters_TheOnTransitionedEventFires()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var triggerX = sm.SetTriggerParameters<string, int, bool>(Trigger.X);

            sm.Configure(State.B)
                .Transition(Trigger.X).To(State.A);

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
            Assert.Equal(3, transition.Parameters.Length);
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
                .Transition(Trigger.X).To(State.A);

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
            Assert.Equal(3, transition.Parameters.Length);
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
                .Transition(Trigger.X).Self()
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
                .Transition(Trigger.X).Self()
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
                .Transition(Trigger.X).Self()
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
            sm.Configure(State.A).Transition(x).To(State.B).If<int>(i => i == 2);
            sm.Fire(x, 2);

            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenParameterizedGuardFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).Transition(x).To(State.B).If<int>(i => i == 3);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void TransitionWhenBothParameterizedGuardClausesTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            var positiveGuard = Tuple.Create(new Func<int, bool>(o => o == 2), "Positive Guard");
            var negativeGuard = Tuple.Create(new Func<int, bool>(o => o != 3), "Negative Guard");
            sm.Configure(State.A).Transition(x).To(State.B).If(positiveGuard, negativeGuard);
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
            sm.Configure(State.A).Transition(x).To(State.B).If(positiveGuard, negativeGuard);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 3));
        }

        [Fact]
        public void TransitionWhenGuardReturnsTrueOnTriggerWithMultipleParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.Configure(State.A).Transition(x).To(State.B).If<string, int>((s, i) => s == "3" && i == 3);
            sm.Fire(x, "3", 3);
            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenGuardFalseOnTriggerWithMultipleParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.Configure(State.A).Transition(x).To(State.B).If<string, int>((s, i) => s == "3" && i == 3);
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
                .Transition(x).To(State.B).If<int>(i => i == 3)
                .Transition(x).To(State.C).If<int>(i => i == 2);
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenPermitIfHasMultipleNonExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(x).To(State.B).If<int>(i => i % 2 == 0)  // Is Even
                .Transition(x).To(State.C).If<int>(i => i == 2);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void TransitionWhenPermitDyanmicIfHasMultipleExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(x).Dynamic<int>(i => i == 3 ? State.B : State.C).If<int>(i => i == 3 || i == 5)
                .Transition(x).Dynamic<int>(i => i == 2 ? State.C : State.D).If<int>(i => i == 2 || i == 4);

            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.B);
        }

        [Fact]
        public void ExceptionWhenPermitDyanmicIfHasMultipleNonExclusiveGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);

            sm.Configure(State.A)
                .Transition(x).Dynamic<int>(i => i == 4 ? State.B : State.C).If<int>(i => i % 2 == 0)
                .Transition(x).Dynamic<int>(i => i == 2 ? State.C : State.D).If<int>(i => i == 2);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(x, 2));
        }

        [Fact]
        public void TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateTrue()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).Transition(x).To( State.D).If<int>( i => i == 3);
            sm.Configure(State.B).SubstateOf(State.A).Transition(x).To(State.C).If<int>( i => i == 2);
            
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.D);
        }

        [Fact]
        public void TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A).Transition(x).To(State.D).If<int>(i => i == 3);
            {
                sm.Configure(State.B).SubstateOf(State.A).Transition(x).To(State.C).If<int>(i => i == 2);
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
                .Transition(x).Self().If<int>(i => i == 3);
            sm.Fire(x, 3);
            Assert.Equal(sm.State, State.A);
        }

        [Fact]
        public void TransitionWhenPermitReentryIfParameterizedGuardFalse()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var x = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(x).Self().If<int>( i => i == 3);

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

            sm.Configure(State.A).Transition(Trigger.X).To(State.B).If(() =>
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
                .Transition(Trigger.X).To(State.B).If(() => i == 2)
                .Transition(Trigger.X).To(State.C).If(() => i == 1);

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
                .Transition(Trigger.Y).To(State.A)
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
                .Transition(Trigger.X).Self()
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
            sm.Configure(State.A).Transition(trigger).To(State.B);
            Assert.True(sm.CanFire(trigger));
        }
        [Fact]
        public void WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(trigger).To(State.B);
            Assert.Single(sm.PermittedTriggers, trigger);
        }


        [Fact]
        public void WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(trigger).To(State.B);
            sm.SetTriggerParameters<string>(trigger);
            Assert.True(sm.CanFire(trigger));
        }
        [Fact]
        public void WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(trigger).To(State.B);
            sm.SetTriggerParameters<string>(trigger);
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(trigger).Internal().Do((_) => { });
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(trigger).Internal().Do((_) => { });
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            var paramTrigger = sm.SetTriggerParameters<string>(trigger);
            sm.Configure(State.A).Transition(trigger).Internal().Do<string>((arg, _) => { });
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            var paramTrigger = sm.SetTriggerParameters<string>(trigger);
            sm.Configure(State.A).Transition(trigger).Internal().Do<string>((arg, _) => { });

            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(sm.SetTriggerParameters<string>(trigger)).To(State.B).If(_ => true);
            Assert.True(sm.CanFire(trigger));
        }

        [Fact]
        public void WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers()
        {
            var trigger = Trigger.X;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Transition(sm.SetTriggerParameters<string>(trigger)).To(State.B).If(_ => true);
            Assert.Single(sm.PermittedTriggers, trigger);
        }

        [Fact]
        public void PermittedTriggersIncludeAllDefinedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.Y).Internal().Do((_) => { })
                .Ignore(Trigger.Z)
                .Transition(Trigger.X).To(State.B);

            Assert.All(new[] { Trigger.X, Trigger.Y, Trigger.Z }, trigger => Assert.Contains(trigger, sm.PermittedTriggers));
        }

        [Fact]
        public void PermittedTriggersExcludeAllUndefinedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B);
            Assert.All(new[] { Trigger.Y, Trigger.Z }, trigger => Assert.DoesNotContain(trigger, sm.PermittedTriggers));
        }

        [Fact]
        public void PermittedTriggersIncludeAllInheritedTriggers()
        {
            State superState = State.A,
                    subState = State.B,
                    otherState = State.C;
            Trigger superStateTrigger = Trigger.X,
                    subStateTrigger = Trigger.Y;

            StateMachine<State, Trigger> hsm(State initialState)
                => new StateMachine<State, Trigger>(initialState)
                        .Configure(superState)
                        .Transition(superStateTrigger).To(otherState)
                    .Machine
                        .Configure(subState)
                        .SubstateOf(superState)
                        .Transition(subStateTrigger).To(otherState)
                    .Machine;

            var hsmInSuperstate = hsm(superState);
            var hsmInSubstate = hsm(subState);

            Assert.All(hsmInSuperstate.PermittedTriggers, trigger => Assert.Contains(trigger, hsmInSubstate.PermittedTriggers));
            Assert.Contains(superStateTrigger, hsmInSubstate.PermittedTriggers);
            Assert.Contains(superStateTrigger, hsmInSuperstate.PermittedTriggers);
            Assert.Contains(subStateTrigger, hsmInSubstate.PermittedTriggers);
            Assert.DoesNotContain(subStateTrigger, hsmInSuperstate.PermittedTriggers);
        }

    }
}