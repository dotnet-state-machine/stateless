﻿using Xunit;
using System;
using System.Threading.Tasks;

namespace Stateless.Tests
{
    public class FluentFixture
    {
        [Fact]
        public void Transition_Returns_TransitionConfiguration()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trans = sm.Configure(State.A).Transition(Trigger.X);

            Assert.NotNull(trans);
            Assert.IsType<StateMachine<State, Trigger>.TransitionConfiguration>(trans);
        }

        [Fact]
        public void Transition_To_Returns_DestinationConfiguration()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trans = sm.Configure(State.A).Transition(Trigger.X).To(State.B);
            Assert.NotNull(trans);
            Assert.IsType<StateMachine<State, Trigger>.DestinationConfiguration>(trans);
        }

        [Fact]
        public void Fire_Transition_To_EndsUpInAnotherState()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).To(State.B);

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Fire_Transition_To_Self()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self();

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal();

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_To_If_True_EndsUpInAnotherState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If((t) => SomethingTrue(t));

            sm.Configure(State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void Fire_Transition_To_If_False_StayInState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If((t) => SomethingFalse(t), "guard description");

            sm.Configure(State.B);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));

            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public void Fire_Transition_Self_If_True_Self()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self().If((t) => SomethingTrue(t));

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Fire_Transition_Self_If_False()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self().If((t) => SomethingFalse(t));

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal_If_True()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal().If((t) => SomethingTrue(t));

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal_If_False()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal().If((t) => SomethingFalse(t));

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_To_DoesAction()
        {
            var _entered = false;
            var _exited = false;
            var _actionWasExecuted = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).To(State.B).Do(() => _actionWasExecuted = true);

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
            Assert.True(_actionWasExecuted);
        }

        [Fact]
        public void Fire_Transition_Self_DoesAction()
        {
            var _entered = false;
            var _exited = false;
            var _actionWasExecuted = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self().Do(() => _actionWasExecuted = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
            Assert.True(_actionWasExecuted);
        }

        [Fact]
        public void Fire_Transition_Internal_DoesAction()
        {
            var _entered = false;
            var _exited = false;
            var _actionWasExecuted = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal().Do(() => _actionWasExecuted = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
            Assert.True(_actionWasExecuted);
        }

        private bool SomethingTrue(object _)
        {
            return true;
        }

        private bool SomethingFalse(object _)
        {
            return false;
        }

        bool _asyncActionWasExecuted = false;
        [Fact]
        public void Fire_Transition_To_DoesAsyncAction()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).To(State.B).Do(() => AsyncAction());

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
            Assert.True(_asyncActionWasExecuted);
        }

        private Task AsyncAction()
        {
            _asyncActionWasExecuted = true;
            return Task.Delay(1);
        }

        [Fact]
        public void Fire_Transition_To_If_TrueReceivedTriggerParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trigger = sm.SetTriggerParameters<string, int, char>(Trigger.X);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If((parameters) => SomethingTrueWithParameters(parameters));

            sm.Configure(State.B);

            sm.Fire(trigger, "arg0", 1, 'c');

            Assert.Equal(State.B, sm.State);
        }
        private bool SomethingTrueWithParameters(object[] parameters)
        {
            Assert.IsType<string>(parameters[0]);
            Assert.IsType<int>(parameters[1]);
            Assert.IsType<char>(parameters[2]);
            return true;
        }

        [Fact]
        public void Fire_Transition_To_If_TrueReceivedOneGenericParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trigger = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If<string>((parameter) => SomethingTrueOneGeneric(parameter));

            sm.Configure(State.B);

            sm.Fire(trigger, "arg0");

            Assert.Equal(State.B, sm.State);
        }

        private bool SomethingTrueOneGeneric(string parameter)
        {
            Assert.IsType<string>(parameter);
            return true;
        }

        bool _actionWasExecuted = false;
        [Fact]
        public void Fire_Transition_To_DoActionReceivesTransition()
        {
            var _entered = false;
            var _exited = false;
            _actionWasExecuted = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).To(State.B).Do((transition) => ActionReceivesTransition(transition));

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
            Assert.True(_actionWasExecuted);
        }

        private void ActionReceivesTransition(StateMachine<State, Trigger>.Transition transition)
        {
            Assert.Equal(State.A, transition.Source);
            Assert.Equal(Trigger.X, transition.Trigger);
            Assert.Equal(State.B, transition.Destination);
            _actionWasExecuted = true;
        }

        [Fact]
        public void Fire_Transition_To_DoActionReceivesTransitionAnsOneParameter()
        {
            var _entered = false;
            var _exited = false;
            _actionWasExecuted = false;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).To(State.B).Do<string>((arg0, transition) => ActionReceivesTransitionAndOneParameter(arg0, transition));

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(trigger, "42");

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
            Assert.True(_actionWasExecuted);
        }

        private void ActionReceivesTransitionAndOneParameter(string arg0, StateMachine<State, Trigger>.Transition _)
        {
            Assert.Equal("42", arg0);
            _actionWasExecuted = true;
        }

        [Fact]
        public void Fire_Transition_Dynamic_EndsUpInAnotherState()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Dynamic(StateSelector);

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        private State StateSelector()
        {
            return State.B;
        }

        [Fact]
        public void Fire_Transition_DynamicWithParameters_EndsUpInAnotherState()
        {
            var _entered = false;
            var _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<string>(Trigger.X);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Dynamic<string>((s) => StateSelectorWithParemeters(s));

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(trigger, "42");

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Configure_Transition_To_Transition_NoGuardOrAction()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B)
                .Transition(Trigger.Y).To(State.C);

            sm.Fire(Trigger.Y);

            Assert.Equal(State.C, sm.State);
        }

        private State StateSelectorWithParemeters(string _)
        {
            return State.B;
        }
    }
}
