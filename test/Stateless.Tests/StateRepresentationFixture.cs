using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class StateRepresentationFixture
    {
        [Test]
        public void UponEntering_EnteringActionsExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddEntryAction((t, a) => actualTransition = t, "entryActionDescription");
            stateRepresentation.Enter(transition);
            Assert.AreEqual(transition, actualTransition);
        }

        [Test]
        public void UponLeaving_EnteringActionsNotExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddEntryAction((t, a) => actualTransition = t, "entryActionDescription");
            stateRepresentation.Exit(transition);
            Assert.IsNull(actualTransition);
        }

        [Test]
        public void UponLeaving_LeavingActionsExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.A);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddExitAction(t => actualTransition = t, "entryActionDescription");
            stateRepresentation.Exit(transition);
            Assert.AreEqual(transition, actualTransition);
        }

        [Test]
        public void UponEntering_LeavingActionsNotExecuted()
        {
            var stateRepresentation = CreateRepresentation(State.A);
            StateMachine<State, Trigger>.Transition
                transition = new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X),
                actualTransition = null;
            stateRepresentation.AddExitAction(t => actualTransition = t, "exitActionDescription");
            stateRepresentation.Enter(transition);
            Assert.IsNull(actualTransition);
        }

        [Test]
        public void IncludesUnderlyingState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.IsTrue(stateRepresentation.Includes(State.B));
        }

        [Test]
        public void DoesNotIncludeUnrelatedState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.IsFalse(stateRepresentation.Includes(State.C));
        }

        [Test]
        public void IncludesSubstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.AddSubstate(CreateRepresentation(State.C));
            Assert.IsTrue(stateRepresentation.Includes(State.C));
        }

        [Test]
        public void DoesNotIncludeSuperstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.Superstate = CreateRepresentation(State.C);
            Assert.IsFalse(stateRepresentation.Includes(State.C));
        }

        [Test]
        public void IsIncludedInUnderlyingState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.IsTrue(stateRepresentation.IsIncludedIn(State.B));
        }

        [Test]
        public void IsNotIncludedInUnrelatedState()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            Assert.IsFalse(stateRepresentation.IsIncludedIn(State.C));
        }

        [Test]
        public void IsNotIncludedInSubstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.AddSubstate(CreateRepresentation(State.C));
            Assert.IsFalse(stateRepresentation.IsIncludedIn(State.C));
        }

        [Test]
        public void IsIncludedInSuperstate()
        {
            var stateRepresentation = CreateRepresentation(State.B);
            stateRepresentation.Superstate = CreateRepresentation(State.C);
            Assert.IsTrue(stateRepresentation.IsIncludedIn(State.C));
        }

        [Test]
        public void WhenTransitioningFromSubToSuperstate_SubstateEntryActionsExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            sub.AddEntryAction((t, a) => executed = true, "entryActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.IsTrue(executed);
        }

        [Test]
        public void WhenTransitioningFromSubToSuperstate_SubstateExitActionsExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            sub.AddExitAction(t => executed = true, "exitActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, super.UnderlyingState, Trigger.X);
            sub.Exit(transition);
            Assert.IsTrue(executed);
        }

        [Test]
        public void WhenTransitioningToSuperFromSubstate_SuperEntryActionsNotExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            super.AddEntryAction((t, a) => executed = true, "entryActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            super.Enter(transition);
            Assert.IsFalse(executed);
        }

        [Test]
        public void WhenTransitioningFromSuperToSubstate_SuperExitActionsNotExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            super.AddExitAction(t => executed = true, "exitActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
            super.Exit(transition);
            Assert.IsFalse(executed);
        }

        [Test]
        public void WhenEnteringSubstate_SuperEntryActionsExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            super.AddEntryAction((t, a) => executed = true, "entryActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.IsTrue(executed);
        }

        [Test]
        public void WhenLeavingSubstate_SuperExitActionsExecuted()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var executed = false;
            super.AddExitAction(t => executed = true, "exitActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
            sub.Exit(transition);
            Assert.IsTrue(executed);
        }

        [Test]
        public void EntryActionsExecuteInOrder()
        {
            var actual = new List<int>();

            var rep = CreateRepresentation(State.B);
            rep.AddEntryAction((t, a) => actual.Add(0), "entryActionDescription");
            rep.AddEntryAction((t, a) => actual.Add(1), "entryActionDescription");

            rep.Enter(new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X));

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(0, actual[0]);
            Assert.AreEqual(1, actual[1]);
        }

        [Test]
        public void ExitActionsExecuteInOrder()
        {
            var actual = new List<int>();

            var rep = CreateRepresentation(State.B);
            rep.AddExitAction(t => actual.Add(0), "entryActionDescription");
            rep.AddExitAction(t => actual.Add(1), "entryActionDescription");

            rep.Exit(new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.X));

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(0, actual[0]);
            Assert.AreEqual(1, actual[1]);
        }

        [Test]
        public void WhenTransitionExists_TriggerCannotBeFired()
        {
            var rep = CreateRepresentation(State.B);
            Assert.IsFalse(rep.CanHandle(Trigger.X));
        }

        [Test]
        public void WhenTransitionDoesNotExist_TriggerCanBeFired()
        {
            var rep = CreateRepresentation(State.B);
            rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, () => true));
            Assert.IsTrue(rep.CanHandle(Trigger.X));
        }

        [Test]
        public void WhenTransitionExistsInSupersate_TriggerCanBeFired()
        {
            var rep = CreateRepresentation(State.B);
            rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, () => true));
            var sub = CreateRepresentation(State.C);
            sub.Superstate = rep;
            rep.AddSubstate(sub);
            Assert.IsTrue(sub.CanHandle(Trigger.X));
        }

        [Test]
        public void WhenEnteringSubstate_SuperstateEntryActionsExecuteBeforeSubstate()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            int order = 0, subOrder = 0, superOrder = 0;
            super.AddEntryAction((t, a) => superOrder = order++, "entryActionDescription");
            sub.AddEntryAction((t, a) => subOrder = order++, "entryActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
            sub.Enter(transition);
            Assert.Less(superOrder, subOrder);
        }

        [Test]
        public void WhenExitingSubstate_SubstateEntryActionsExecuteBeforeSuperstate()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            int order = 0, subOrder = 0, superOrder = 0;
            super.AddExitAction(t => superOrder = order++, "entryActionDescription");
            sub.AddExitAction(t => subOrder = order++, "entryActionDescription");
            var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
            sub.Exit(transition);
            Assert.Less(subOrder, superOrder);
        }

        [Test]
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

            Assert.IsFalse(rep.CanHandle(Trigger.X));
        }

        [Test]
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

            Assert.IsTrue(rep.CanHandle(Trigger.X));
        }

        [Test]
        public void WhenTransitionExistAndSuperstateUnmetGuardConditions_FireNotPossible()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var falseConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };
            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(falseConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
            super.AddTriggerBehaviour(transition);

            StateMachine<State, Trigger>.TriggerBehaviourResult result;
            sub.TryFindHandler(Trigger.X, out result);

            Assert.False(sub.CanHandle(Trigger.X));
            Assert.False(super.CanHandle(Trigger.X));
            Assert.IsNotNull(result);
            Assert.False(result?.Handler.GuardConditionsMet);
            Assert.Contains("2", result?.UnmetGuardConditions.ToArray());
            
        }
        [Test]
        public void WhenTransitionExistSuperstateMetGuardConditions_CanBeFired()
        {
            StateMachine<State, Trigger>.StateRepresentation super;
            StateMachine<State, Trigger>.StateRepresentation sub;
            CreateSuperSubstatePair(out super, out sub);

            var trueConditions = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };
            var transitionGuard = new StateMachine<State, Trigger>.TransitionGuard(trueConditions);
            var transition = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);

            super.AddTriggerBehaviour(transition);
            StateMachine<State, Trigger>.TriggerBehaviourResult result;
            sub.TryFindHandler(Trigger.X, out result);

            Assert.True(sub.CanHandle(Trigger.X));
            Assert.True(super.CanHandle(Trigger.X));
            Assert.IsNotNull(result);     
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
