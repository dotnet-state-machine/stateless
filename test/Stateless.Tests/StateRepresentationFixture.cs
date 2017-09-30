using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Stateless.Tests
{

    public class StateRepresentationFixture
    {
        [Fact]
        public void UponEntering_EnteringActionsExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddEntryAction((t, a) => actualTransition = t, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            stateRepresentation.Enter(transition);
            Assert.Equal(transition, actualTransition);
        }

        [Fact]
        public void UponLeaving_EnteringActionsNotExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddEntryAction((t, a) => actualTransition = t, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            stateRepresentation.Exit(transition);
            Assert.Null(actualTransition);
        }

        [Fact]
        public void UponLeaving_LeavingActionsExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.A);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddExitAction(t => actualTransition = t, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            stateRepresentation.Exit(transition);
            Assert.Equal(transition, actualTransition);
        }

        [Fact]
        public void UponEntering_LeavingActionsNotExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.A);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddExitAction(t => actualTransition = t, Reflection.InvocationInfo.Create(null, "exitActionDescription"));
            stateRepresentation.Enter(transition);
            Assert.Null(actualTransition);
        }

        [Fact]
        public void IncludesUnderlyingState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.True(stateRepresentation.Includes(State.B));
        }

        [Fact]
        public void DoesNotIncludeUnrelatedState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.False(stateRepresentation.Includes(State.C));
        }

        [Fact]
        public void IncludesSubstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.AddSubstate(CreateRepresentation(State.C));
            Assert.True(stateRepresentation.Includes(State.C));
        }

        [Fact]
        public void DoesNotIncludeSuperstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.Superstate = CreateRepresentation(State.C);
            Assert.False(stateRepresentation.Includes(State.C));
        }

        [Fact]
        public void IsIncludedInUnderlyingState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.True(stateRepresentation.IsIncludedIn(State.B));
        }

        [Fact]
        public void IsNotIncludedInUnrelatedState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.False(stateRepresentation.IsIncludedIn(State.C));
        }

        [Fact]
        public void IsNotIncludedInSubstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.AddSubstate(CreateRepresentation(State.C));
            Assert.False(stateRepresentation.IsIncludedIn(State.C));
        }

        [Fact]
        public void IsIncludedInSuperstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.Superstate = CreateRepresentation(State.C);
            Assert.True(stateRepresentation.IsIncludedIn(State.C));
        }

        [Fact]
        public void WhenTransitioningFromSubToSuperstate_SubstateEntryActionsExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            sub.AddEntryAction((t, a) => executed = true, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.True(executed);
        }

        [Fact]
        public void WhenTransitioningFromSubToSuperstate_SubstateExitActionsExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            sub.AddExitAction(t => executed = true, Reflection.InvocationInfo.Create(null, "exitActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, super.UnderlyingState, Trigger.X);
            sub.Exit(transition);
            Assert.True(executed);
        }

        [Fact]
        public void WhenTransitioningToSuperFromSubstate_SuperEntryActionsNotExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            super.AddEntryAction((t, a) => executed = true, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            super.Enter(transition);
            Assert.False(executed);
        }

        [Fact]
        public void WhenTransitioningFromSuperToSubstate_SuperExitActionsNotExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            super.AddExitAction(t => executed = true, Reflection.InvocationInfo.Create(null, "exitActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            super.Exit(transition);
            Assert.False(executed);
        }

        [Fact]
        public void WhenEnteringSubstate_SuperEntryActionsExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            super.AddEntryAction((t, a) => executed = true, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.True(executed);
        }

        [Fact]
        public void WhenLeavingSubstate_SuperExitActionsExecuted()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var executed = false;
            super.AddExitAction(t => executed = true, Reflection.InvocationInfo.Create(null, "exitActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
            sub.Exit(transition);
            Assert.True(executed);
        }

        [Fact]
        public void EntryActionsExecuteInOrder()
        {
            var actual = new List<int>();

            var rep = CreateRepresentation(State.B);
            rep.AddEntryAction((t, a) => actual.Add(0), Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            rep.AddEntryAction((t, a) => actual.Add(1), Reflection.InvocationInfo.Create(null, "entryActionDescription"));

            rep.Enter(new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X));

            Assert.Equal(2, actual.Count);
            Assert.Equal(0, actual[0]);
            Assert.Equal(1, actual[1]);
        }

        [Fact]
        public void ExitActionsExecuteInOrder()
        {
            var actual = new List<int>();

            var rep = CreateRepresentation(State.B);
            rep.AddExitAction(t => actual.Add(0), Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            rep.AddExitAction(t => actual.Add(1), Reflection.InvocationInfo.Create(null, "entryActionDescription"));

            rep.Exit(new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.X));

            Assert.Equal(2, actual.Count);
            Assert.Equal(0, actual[0]);
            Assert.Equal(1, actual[1]);
        }

        [Fact]
        public void WhenTransitionExists_TriggerCannotBeFired()
        {
            var rep = CreateRepresentation(State.B);
            Assert.False(rep.CanHandle(Trigger.X));
        }

        [Fact]
        public void WhenTransitionDoesNotExist_TriggerCanBeFired()
        {
            var rep = CreateRepresentation(State.B);
            rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null));
            Assert.True(rep.CanHandle(Trigger.X));
        }

        [Fact]
        public void WhenTransitionExistsInSupersate_TriggerCanBeFired()
        {
            var rep = CreateRepresentation(State.B);
            rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null));
            var sub = CreateRepresentation(State.C);
            sub.Superstate = rep;
            rep.AddSubstate(sub);
            Assert.True(sub.CanHandle(Trigger.X));
        }

        [Fact]
        public void WhenEnteringSubstate_SuperstateEntryActionsExecuteBeforeSubstate()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            int order = 0, subOrder = 0, superOrder = 0;
            super.AddEntryAction((t, a) => superOrder = order++, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            sub.AddEntryAction((t, a) => subOrder = order++, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.True(superOrder < subOrder);
        }

        [Fact]
        public void WhenExitingSubstate_SubstateEntryActionsExecuteBeforeSuperstate()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            int order = 0, subOrder = 0, superOrder = 0;
            super.AddExitAction(t => superOrder = order++, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            sub.AddExitAction(t => subOrder = order++, Reflection.InvocationInfo.Create(null, "entryActionDescription"));
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
            sub.Exit(transition);
            Assert.True(subOrder < superOrder);
        }

        [Fact]
        public void WhenTransitionUnmetGuardConditions_TriggerCannotBeFired()
        {
            var rep = CreateRepresentation(State.B);

            var falseConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };

            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(falseConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
            rep.AddTriggerBehaviour(transition);

            Assert.False(rep.CanHandle(Trigger.X));
        }

        [Fact]
        public void WhenTransitioGuardConditionsMet_TriggerCanBeFired()
        {
            var rep = CreateRepresentation(State.B);

            var trueConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(trueConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
            rep.AddTriggerBehaviour(transition);

            Assert.True(rep.CanHandle(Trigger.X));
        }

        [Fact]
        public void WhenTransitionExistAndSuperstateUnmetGuardConditions_FireNotPossible()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var falseConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };
            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(falseConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
            super.AddTriggerBehaviour(transition);

            sub.TryFindHandler(Trigger.X, out StateMachine<State, Trigger>.TriggerBehaviourResult result);

            Assert.False(sub.CanHandle(Trigger.X));
            Assert.False(super.CanHandle(Trigger.X));
            Assert.NotNull(result);
            Assert.False(result?.Handler.GuardConditionsMet);
            Assert.Contains("2", result?.UnmetGuardConditions.ToArray());
            
        }
        [Fact]
        public void WhenTransitionExistSuperstateMetGuardConditions_CanBeFired()
        {
            CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub);

            var trueConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };
            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(trueConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);

            super.AddTriggerBehaviour(transition);
            sub.TryFindHandler(Trigger.X, out StateMachine<State, Trigger>.TriggerBehaviourResult result);

            Assert.True(sub.CanHandle(Trigger.X));
            Assert.True(super.CanHandle(Trigger.X));
            Assert.NotNull(result);     
            Assert.True(result?.Handler.GuardConditionsMet);
            Assert.False(result?.UnmetGuardConditions.Any());

        }

        void CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super, out StateMachine<State, Trigger>.StateRepresentation sub)
        {
            super = CreateRepresentation(State.A);
            sub = CreateRepresentation(State.B);
            super.AddSubstate(sub);
            sub.Superstate = super;
        }

        StateMachine<State, Trigger>.StateRepresentation CreateRepresentation(State state)
        {
            return new StateMachine<State, Trigger>.StateRepresentation(state);
        }
    }
}
