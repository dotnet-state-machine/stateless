﻿using System.Diagnostics;
using Stateless.Reflection;
using Xunit.Sdk;
// ReSharper disable ConvertClosureToMethodGroup

namespace Stateless.Tests;

public class ReflectionFixture {
    private static readonly string UserDescription = "UserDescription";

    private bool IsTrue() => true;

    private void OnEntry() { }

    private void OnEntryInt(int i) { }

    private void OnEntryIntInt(int i, int j) { }

    private void OnEntryIntIntInt(int i, int j, int k) { }

    private void OnEntryTrans(StateMachine<State, Trigger>.Transition trans) { }

    private void OnEntryIntTrans(int i, StateMachine<State, Trigger>.Transition trans) { }

    private void OnExit() { }

    private Task OnActivateAsync() => TaskResult.Done;

    private void OnActivate() { }

    private Task OnEntryTransAsync(StateMachine<State, Trigger>.Transition trans) => TaskResult.Done;

    private Task OnEntryAsync() => TaskResult.Done;

    private Task OnDeactivateAsync() => TaskResult.Done;

    private void OnDeactivate() { }

    private void OnExitTrans(StateMachine<State, Trigger>.Transition trans) { }

    private Task OnExitAsync() => TaskResult.Done;

    private Task OnExitTransAsync(StateMachine<State, Trigger>.Transition trans) => TaskResult.Done;

    private bool Permit() => true;

    private static void VerifyMethodNames(IEnumerable<InvocationInfo> methods, string prefix, string body, State state,
                                          InvocationInfo.Timing       timing) {
        Assert.Equal(1, methods.Count());
        var method = methods.First();

        if (state == State.A)
            Assert.Equal(prefix + body + (timing == InvocationInfo.Timing.Asynchronous ? "Async" : ""),
                         method.Description);
        else if (state == State.B)
            Assert.Equal($"{UserDescription}B-{body}", method.Description);
        else if (state == State.C)
            Assert.Equal(InvocationInfo.DefaultFunctionDescription, method.Description);
        else if (state == State.D)
            Assert.Equal($"{UserDescription}D-{body}", method.Description);

        Assert.Equal(timing == InvocationInfo.Timing.Asynchronous, method.IsAsync);
    }

    private static void VerifyMethodNames(IEnumerable<InvocationInfo> methods, string prefix, string body,
                                            State                       state,
                                            InvocationInfo.Timing       timing, HashSet<string> suffixes) {
        Assert.Equal(suffixes.Count, methods.Count());

        foreach (var method in methods) {
            Debug.WriteLine($"Method description is \"{method.Description}\"");
            //
            var matches = false;
            foreach (var suffix in suffixes) {
                if (state == State.A)
                    matches = method.Description == prefix + body
                                                           + (timing == InvocationInfo.Timing.Asynchronous
                                                                  ? "Async"
                                                                  : $"{suffix}");
                else if (state == State.B)
                    matches = $"{UserDescription}B-{body}{suffix}" == method.Description;
                else if (state == State.C)
                    matches = InvocationInfo.DefaultFunctionDescription == method.Description;
                else if (state == State.D)
                    matches = $"{UserDescription}D-{body}{suffix}" == method.Description;
                //
                if (matches) {
                    suffixes.Remove(suffix);
                    break;
                }
            }

            if (!matches)
                Debug.WriteLine($"No match for \"{method.Description}\"");
            Assert.True(matches);
            //
            Assert.Equal(timing == InvocationInfo.Timing.Asynchronous, method.IsAsync);
        }
    }

    private State NextState() => State.D;

    [Fact]
    public void DestinationStateIsCalculatedBasedOnTriggerParameters_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var trigger = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding transition count mismatch"
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(1, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        foreach (var trans in binding.DynamicTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
        }
    }

    [Fact]
    public void DestinationStateIsDynamic_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .PermitDynamic(Trigger.X, () => State.B);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding transition count mismatch
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(1, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        foreach (var trans in binding.DynamicTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
        }
    }

    [Fact]
    public void OnEntryWithAnonymousActionAndDescription_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntry(() => { }, "enteredA");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(1, binding.EntryActions.Count());
        foreach (var entryAction in binding.EntryActions) Assert.Equal("enteredA", entryAction.Method.Description);
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
    }

    [Fact]
    public void OnEntryWithNamedDelegateActionAndDescription_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntry(OnEntry, "enteredA");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        //
        Assert.Equal(1, binding.EntryActions.Count());
        foreach (var entryAction in binding.EntryActions)
            Assert.Equal("enteredA", entryAction.Method.Description);
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
    }

    [Fact]
    public void OnExitWithAnonymousActionAndDescription_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnExit(() => { }, "exitA");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        //
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(1, binding.ExitActions.Count());
        foreach (var exitAction in binding.ExitActions)
            Assert.Equal("exitA", exitAction.Description);
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
    }

    [Fact]
    public void OnExitWithNamedDelegateActionAndDescription_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnExit(OnExit, "exitA");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.States.Count(), 1);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        //
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(1, binding.ExitActions.Count());
        foreach (var entryAction in binding.ExitActions)
            Assert.Equal("exitA", entryAction.Description);
        //
        Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
    }

    [Fact]
    public void ReflectionMethodNames() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnActivate(OnActivate)
          .OnEntry(OnEntry)
          .OnExit(OnExit)
          .OnDeactivate(OnDeactivate);
        sm.Configure(State.B)
          .OnActivate(OnActivate, $"{UserDescription}B-Activate")
          .OnEntry(OnEntry, $"{UserDescription}B-Entry")
          .OnExit(OnExit, $"{UserDescription}B-Exit")
          .OnDeactivate(OnDeactivate, $"{UserDescription}B-Deactivate");
        sm.Configure(State.C)
          .OnActivate(() => OnActivate())
          .OnEntry(() => OnEntry())
          .OnExit(() => OnExit())
          .OnDeactivate(() => OnDeactivate());
        sm.Configure(State.D)
          .OnActivate(() => OnActivate(), $"{UserDescription}D-Activate")
          .OnEntry(() => OnEntry(), $"{UserDescription}D-Entry")
          .OnExit(() => OnExit(), $"{UserDescription}D-Exit")
          .OnDeactivate(() => OnDeactivate(), $"{UserDescription}D-Deactivate");

        var inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            VerifyMethodNames(stateInfo.ActivateActions, "On", "Activate", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Synchronous);
            VerifyMethodNames(stateInfo.EntryActions.Select(x => x.Method), "On", "Entry",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Synchronous);
            VerifyMethodNames(stateInfo.ExitActions, "On", "Exit", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Synchronous);
            VerifyMethodNames(stateInfo.DeactivateActions, "On", "Deactivate", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Synchronous);
        }

        // --------------------------------------------------------

        // New StateMachine, new tests: entry and exit, functions that take the transition as an argument
        sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntry(OnEntryTrans)
          .OnExit(OnExitTrans);
        sm.Configure(State.B)
          .OnEntry(OnEntryTrans, $"{UserDescription}B-EntryTrans")
          .OnExit(OnExitTrans, $"{UserDescription}B-ExitTrans");
        sm.Configure(State.C)
          .OnEntry(t => OnEntryTrans(t))
          .OnExit(t => OnExitTrans(t));
        sm.Configure(State.D)
          .OnEntry(t => OnEntryTrans(t), $"{UserDescription}D-EntryTrans")
          .OnExit(t => OnExitTrans(t), $"{UserDescription}D-ExitTrans");

        inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            VerifyMethodNames(stateInfo.EntryActions.Select(x => x.Method), "On", "EntryTrans",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Synchronous);
            VerifyMethodNames(stateInfo.ExitActions, "On", "ExitTrans", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Synchronous);
        }

        // --------------------------------------------------------

        sm = new StateMachine<State, Trigger>(State.A);

        var triggerX = sm.SetTriggerParameters<int>(Trigger.X);
        var triggerY = sm.SetTriggerParameters<int, int>(Trigger.Y);
        var triggerZ = sm.SetTriggerParameters<int, int, int>(Trigger.Z);

        sm.Configure(State.A)
          .OnEntryFrom(Trigger.X, OnEntry)
          .OnEntryFrom(Trigger.Y, OnEntryTrans)
          .OnEntryFrom(triggerX, OnEntryInt)
          .OnEntryFrom(triggerX, OnEntryIntTrans)
          .OnEntryFrom(triggerY, OnEntryIntInt)
          .OnEntryFrom(triggerZ, OnEntryIntIntInt);
        sm.Configure(State.B)
          .OnEntryFrom(Trigger.X, OnEntry, $"{UserDescription}B-Entry")
          .OnEntryFrom(Trigger.Y, OnEntryTrans, $"{UserDescription}B-EntryTrans")
          .OnEntryFrom(triggerX, OnEntryInt, $"{UserDescription}B-EntryInt")
          .OnEntryFrom(triggerX, OnEntryIntTrans, $"{UserDescription}B-EntryIntTrans")
          .OnEntryFrom(triggerY, OnEntryIntInt, $"{UserDescription}B-EntryIntInt")
          .OnEntryFrom(triggerZ, OnEntryIntIntInt, $"{UserDescription}B-EntryIntIntInt");
        sm.Configure(State.C)
          .OnEntryFrom(Trigger.X, () => OnEntry())
          .OnEntryFrom(Trigger.Y, trans => OnEntryTrans(trans))
          .OnEntryFrom(triggerX, i => OnEntryInt(i))
          .OnEntryFrom(triggerX, (i, trans) => OnEntryIntTrans(i, trans))
          .OnEntryFrom(triggerY, (i, j) => OnEntryIntInt(i, j))
          .OnEntryFrom(triggerZ, (i, j, k) => OnEntryIntIntInt(i, j, k));
        sm.Configure(State.D)
          .OnEntryFrom(Trigger.X, () => OnEntry(), $"{UserDescription}D-Entry")
          .OnEntryFrom(Trigger.Y, trans => OnEntryTrans(trans), $"{UserDescription}D-EntryTrans")
          .OnEntryFrom(triggerX, i => OnEntryInt(i), $"{UserDescription}D-EntryInt")
          .OnEntryFrom(triggerX, (i, trans) => OnEntryIntTrans(i, trans), $"{UserDescription}D-EntryIntTrans")
          .OnEntryFrom(triggerY, (i, j) => OnEntryIntInt(i, j), $"{UserDescription}D-EntryIntInt")
          .OnEntryFrom(triggerZ, (i, j, k) => OnEntryIntIntInt(i, j, k), $"{UserDescription}D-EntryIntIntInt");

        inf = sm.GetInfo();

        foreach (var stateInfo in inf.States)
            VerifyMethodNames(stateInfo.EntryActions.Select(x => x.Method), "On", "Entry",
                                (State) stateInfo.UnderlyingState,
                                InvocationInfo.Timing.Synchronous,
                                new HashSet<string> {
                                    "",
                                    "Trans",
                                    "Int",
                                    "IntTrans",
                                    "IntInt",
                                    "IntIntInt"
                                });

        /*
        public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1, Transition> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2, Transition> entryAction, string entryActionDescription = null)
         */
    }

    [Fact]
    public void ReflectionMethodNamesAsync() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnActivateAsync(OnActivateAsync)
          .OnEntryAsync(OnEntryAsync)
          .OnExitAsync(OnExitAsync)
          .OnDeactivateAsync(OnDeactivateAsync);
        sm.Configure(State.B)
          .OnActivateAsync(OnActivateAsync, $"{UserDescription}B-Activate")
          .OnEntryAsync(OnEntryAsync, $"{UserDescription}B-Entry")
          .OnExitAsync(OnExitAsync, $"{UserDescription}B-Exit")
          .OnDeactivateAsync(OnDeactivateAsync, $"{UserDescription}B-Deactivate");
        sm.Configure(State.C)
          .OnActivateAsync(() => OnActivateAsync())
          .OnEntryAsync(() => OnEntryAsync())
          .OnExitAsync(() => OnExitAsync())
          .OnDeactivateAsync(() => OnDeactivateAsync());
        sm.Configure(State.D)
          .OnActivateAsync(() => OnActivateAsync(), $"{UserDescription}D-Activate")
          .OnEntryAsync(() => OnEntryAsync(), $"{UserDescription}D-Entry")
          .OnExitAsync(() => OnExitAsync(), $"{UserDescription}D-Exit")
          .OnDeactivateAsync(() => OnDeactivateAsync(), $"{UserDescription}D-Deactivate");

        var inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            VerifyMethodNames(stateInfo.ActivateActions, "On", "Activate", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Asynchronous);
            VerifyMethodNames(stateInfo.EntryActions.Select(x => x.Method), "On", "Entry",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Asynchronous);
            VerifyMethodNames(stateInfo.ExitActions, "On", "Exit", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Asynchronous);
            VerifyMethodNames(stateInfo.DeactivateActions, "On", "Deactivate", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Asynchronous);
        }

        // New StateMachine, new tests: entry and exit, functions that take the transition as an argument
        sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntryAsync(OnEntryTransAsync)
          .OnExitAsync(OnExitTransAsync);
        sm.Configure(State.B)
          .OnEntryAsync(OnEntryTransAsync, $"{UserDescription}B-EntryTrans")
          .OnExitAsync(OnExitTransAsync, $"{UserDescription}B-ExitTrans");
        sm.Configure(State.C)
          .OnEntryAsync(t => OnEntryTransAsync(t))
          .OnExitAsync(t => OnExitTransAsync(t));
        sm.Configure(State.D)
          .OnEntryAsync(t => OnEntryTransAsync(t), $"{UserDescription}D-EntryTrans")
          .OnExitAsync(t => OnExitTransAsync(t), $"{UserDescription}D-ExitTrans");

        inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            VerifyMethodNames(stateInfo.EntryActions.Select(x => x.Method), "On", "EntryTrans",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Asynchronous);
            VerifyMethodNames(stateInfo.ExitActions, "On", "ExitTrans", (State) stateInfo.UnderlyingState,
                              InvocationInfo.Timing.Asynchronous);
        }
        /*
        public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Transition, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Transition, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Transition, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task> entryAction, string entryActionDescription = null)
        public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Transition, Task> entryAction, string entryActionDescription = null)
        */
    }

    [Fact]
    public void SimpleTransition_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .Permit(Trigger.X, State.B);

        var inf = sm.GetInfo();

        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(1, binding.FixedTransitions.Count());
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
        }

        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }

    [Fact]
    public void TransitionGuardNames() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, Permit);
        sm.Configure(State.B)
          .PermitIf(Trigger.X, State.C, Permit, $"{UserDescription}B-Permit");
        sm.Configure(State.C)
          .PermitIf(Trigger.X, State.B, () => Permit());
        sm.Configure(State.D)
          .PermitIf(Trigger.X, State.C, () => Permit(), $"{UserDescription}D-Permit");

        var inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            Assert.Equal(1, stateInfo.Transitions.Count());
            var transInfo = stateInfo.Transitions.First();
            Assert.Equal(1, transInfo.GuardConditionsMethodDescriptions.Count());
            VerifyMethodNames(transInfo.GuardConditionsMethodDescriptions, "", "Permit",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Synchronous);
        }


        // --------------------------------------------------------

        sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitDynamicIf(Trigger.X, NextState, Permit);
        sm.Configure(State.B)
          .PermitDynamicIf(Trigger.X, NextState, Permit, $"{UserDescription}B-Permit");
        sm.Configure(State.C)
          .PermitDynamicIf(Trigger.X, NextState, () => Permit());
        sm.Configure(State.D)
          .PermitDynamicIf(Trigger.X, NextState, () => Permit(), $"{UserDescription}D-Permit");

        inf = sm.GetInfo();

        foreach (var stateInfo in inf.States) {
            Assert.Equal(1, stateInfo.Transitions.Count());
            var transInfo = stateInfo.Transitions.First();
            Assert.Equal(1, transInfo.GuardConditionsMethodDescriptions.Count());
            VerifyMethodNames(transInfo.GuardConditionsMethodDescriptions, "", "Permit",
                              (State) stateInfo.UnderlyingState, InvocationInfo.Timing.Synchronous);
        }

        /*
       public IgnoredTriggerBehaviour(TTrigger trigger, Func<bool> guard, string description = null)
           : base(trigger, new TransitionGuard(guard, description))
        public InternalTriggerBehaviour(TTrigger trigger, Func<bool> guard)
            : base(trigger, new TransitionGuard(guard, "Internal Transition"))
        public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard = null, string guardDescription = null)
            : base(trigger, new TransitionGuard(guard, guardDescription))

        public StateConfiguration PermitReentryIf(TTrigger trigger, Func<bool> guard, string guardDescription = null)

        public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
        public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
        public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)

        StateConfiguration InternalPermit(TTrigger trigger, TState destinationState, string guardDescription)
        StateConfiguration InternalPermitDynamic(TTrigger trigger, Func<object[], TState> destinationStateSelector, string guardDescription)
         */
    }

    [Fact]
    public void TransitionWithIgnore_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .Ignore(Trigger.Y)
          .Permit(Trigger.X, State.B);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(1, binding.FixedTransitions.Count()); // Transition count mismatch"
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
        }

        //
        Assert.Equal(1, binding.IgnoredTriggers.Count()); //  Ignored triggers count mismatch
        foreach (var ignore in binding.IgnoredTriggers) {
            Assert.True(ignore.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.Y, (Trigger) ignore.Trigger.UnderlyingTrigger); // Ignored trigger value mismatch
        }

        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
    }

    [Fact]
    public void TwoSimpleTransitions_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .Permit(Trigger.X, State.B)
          .Permit(Trigger.Y, State.C);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 3);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState); // Binding state value mismatch
        //
        Assert.Equal(0, binding.Substates.Count()); //  Binding substate count mismatch"
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count()); //  Binding entry actions count mismatch
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(2, binding.FixedTransitions.Count()); // Transition count mismatch
        //
        var haveXb = false;
        var haveYc = false;
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
            //
            // Can't make assumptions about which trigger/destination comes first in the list
            if ((Trigger) trans.Trigger.UnderlyingTrigger == Trigger.X) {
                Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
                Assert.False(haveXb);
                haveXb = true;
            } else if ((Trigger) trans.Trigger.UnderlyingTrigger == Trigger.Y) {
                Assert.Equal(State.C, (State) trans.DestinationState.UnderlyingState);
                Assert.False(haveYc);
                haveYc = true;
            } else {
                throw new XunitException("Failed.");
            }
        }

        Assert.True(haveXb && haveYc);
        //
        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }

    [Fact]
    public void WhenDiscriminatedByAnonymousGuard_Binding() {
        static bool AnonymousGuard() => true;

        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, AnonymousGuard);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(1, binding.FixedTransitions.Count());
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            //
            Assert.NotEqual(0, trans.GuardConditionsMethodDescriptions.Count());
        }

        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }

    [Fact]
    public void WhenDiscriminatedByAnonymousGuardWithDescription_Binding() {
        static bool AnonymousGuard() => true;

        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, AnonymousGuard, "description");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(1, binding.FixedTransitions.Count());
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            //
            Assert.Equal(1, trans.GuardConditionsMethodDescriptions.Count());
            Assert.Equal("description", trans.GuardConditionsMethodDescriptions.First().Description);
        }

        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }

    [Fact]
    public void WhenDiscriminatedByNamedDelegate_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, IsTrue);

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(1, binding.FixedTransitions.Count());
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            //
            Assert.Equal(1, trans.GuardConditionsMethodDescriptions.Count());
            Assert.Equal("IsTrue", trans.GuardConditionsMethodDescriptions.First().Description);
        }

        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }

    [Fact]
    public void WhenDiscriminatedByNamedDelegateWithDescription_Binding() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, IsTrue, "description");

        var inf = sm.GetInfo();

        Assert.True(inf.StateType == typeof(State));
        Assert.Equal(inf.TriggerType, typeof(Trigger));
        Assert.Equal(inf.States.Count(), 2);
        var binding = inf.States.Single(s => (State) s.UnderlyingState == State.A);

        Assert.True(binding.UnderlyingState is State);
        Assert.Equal(State.A, (State) binding.UnderlyingState);
        //
        Assert.Equal(0, binding.Substates.Count());
        Assert.Equal(null, binding.Superstate);
        Assert.Equal(0, binding.EntryActions.Count());
        Assert.Equal(0, binding.ExitActions.Count());
        //
        Assert.Equal(1, binding.FixedTransitions.Count());
        foreach (var trans in binding.FixedTransitions) {
            Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
            Assert.Equal(Trigger.X, (Trigger) trans.Trigger.UnderlyingTrigger);
            //
            Assert.True(trans.DestinationState.UnderlyingState is State);
            Assert.Equal(State.B, (State) trans.DestinationState.UnderlyingState);
            //
            Assert.Equal(1, trans.GuardConditionsMethodDescriptions.Count());
            Assert.Equal("description", trans.GuardConditionsMethodDescriptions.First().Description);
        }

        Assert.Equal(0, binding.IgnoredTriggers.Count());
        Assert.Equal(0, binding.DynamicTransitions.Count());
    }
}