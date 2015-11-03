**Create *state machines* and lightweight *state machine-based workflows* directly in .NET code:**

```csharp
var phoneCall = new StateMachine<State, Trigger>(State.OffHook);

phoneCall.Configure(State.OffHook)
    .Permit(Trigger.CallDialed, State.Ringing);
	
phoneCall.Configure(State.Ringing)
    .Permit(Trigger.HungUp, State.OffHook)
    .Permit(Trigger.CallConnected, State.Connected);
 
phoneCall.Configure(State.Connected)
    .OnEntry(() => StartCallTimer())
    .OnExit(() => StopCallTimer())
    .Permit(Trigger.LeftMessage, State.OffHook)
    .Permit(Trigger.HungUp, State.OffHook)
    .Permit(Trigger.PlacedOnHold, State.OnHold);

// ...

phoneCall.Fire(Trigger.CallDialled);
Assert.AreEqual(State.Ringing, phoneCall.State);
```

This project, as well as the example above, was inspired by [Simple State Machine](http://simplestatemachine.codeplex.com/).

##Features

Most standard state machine constructs are supported:

 * Generic support for states and triggers of any .NET type (numbers, strings, enums, etc.)
 * Hierarchical states
 * Entry/exit events for states
 * Guard clauses to support conditional transitions
 * Introspection

Some useful extensions are also provided:

 * Ability to store state externally (for example, in a property tracked by an ORM)
 * Parameterised triggers
 * Reentrant states
 * Export to DOT graph


###Hierarchical States

In the example below, the `OnHold` state is a substate of the `Connected` state. This means that an `OnHold` call is still connected.

```csharp
phoneCall.Configure(State.OnHold)
    .SubstateOf(State.Connected)
    .Permit(Trigger.TakenOffHold, State.Connected)
    .Permit(Trigger.HungUp, State.OffHook)
    .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);
```

In addition to the `StateMachine.State` property, which will report the precise current state, an `IsInState(State)` method is provided. `IsInState(State)` will take substates into account, so that if the example above was in the `OnHold` state, `IsInState(State.Connected)` would also evaluate to `true`.

###Entry/Exit Events

In the example, the `StartCallTimer()` method will be executed when a call is connected. The `StopCallTimer()` will be executed when call completes (by either hanging up or hurling the phone against the wall.)

The call can move between the `Connected` and `OnHold` states without the `StartCallTimer()` and `StopCallTimer()` methods being called repeatedly because the `OnHold` state is a substate of the `Connected` state.

Entry/Exit event handlers can be supplied with a parameter of type `Transition` that describes the trigger, source and destination states.

###External State Storage

Stateless has been designed with encapsulation within an ORM-ed domain model in mind. Some ORMs place requirements upon where mapped data may be stored. To this end, the `StateMachine` constructor can accept function arguments that will be used to read and write the state values:

```csharp
var stateMachine = new StateMachine<State, Trigger>(
    () => myState.Value,
    s => myState.Value = s);
```

In this example the state machine will use the `myState` object for state storage.

###Introspection

The state machine can provide a list of the triggers than can be successfully fired within the current state via the `StateMachine.PermittedTriggers` property.

###Guard Clauses

The state machine will choose between multiple transitions based on guard clauses, e.g.:

```csharp
phoneCall.Configure(State.OffHook)
    .PermitIf(Trigger.CallDialled, State.Ringing, () => IsValidNumber)
    .PermitIf(Trigger.CallDialled, State.Beeping, () => !IsValidNumber);
```

Guard clauses within a state must be mutually exclusive (multiple guard clauses cannot be valid at the same time.) Substates can override transitions by respecifying them, however substates cannot disallow transitions that are allowed by the superstate.

###Parameterised Triggers

Strongly-typed parameters can be assigned to triggers:

```csharp
var assignTrigger = stateMachine.SetTriggerParameters<string>(Trigger.Assign);

stateMachine.Configure(State.Assigned)
    .OnEntryFrom(assignTrigger, email => OnAssigned(email));

stateMachine.Fire(assignTrigger, "joe@example.com");
```

Trigger parameters can be used to dynamically select the destination state using the `PermitDynamic()` configuration method.

###Ignored Transitions and Reentrant States

Firing a trigger that does not have an allowed transition associated with it will cause an exception to be thrown.

To ignore triggers within certain states, use the `Ignore(TTrigger)` directive:

```csharp
phoneCall.Configure(State.Connected)
    .Ignore(Trigger.CallDialled);
```

Alternatively, a state can be marked reentrant so its entry and exit events will fire even when transitioning from/to itself:

```csharp
stateMachine.Configure(State.Assigned)
    .PermitReentry(Trigger.Assigned)
    .OnEntry(() => SendEmailToAssignee());
```

By default, triggers must be ignored explicitly. To override Stateless's default behaviour of throwing an exception when an unhandled trigger is fired, configure the state machine using the `OnUnhandledTrigger` method:

```csharp
stateMachine.OnUnhandledTrigger((state, trigger) => { });
```

###Export to DOT graph

It can be useful to visualize state machines on runtime. With this approach the code is the authoritative source and state diagrams are by-products which are always up to date.
 
```csharp
phoneCall.Configure(State.OffHook)
    .PermitIf(Trigger.CallDialed, State.Ringing, IsValidNumber);
string graph = phoneCall.ToDotGraph();
```

The `StateMachine.ToDotGraph()` method returns a string representation of the state machine in the [DOT graph language](https://en.wikipedia.org/wiki/DOT_(graph_description_language)), e.g.:

```dot
digraph {
 OffHook -> Ringing [label="CallDialed [IsValidNumber]"];
}
```

This can then be rendered by tools that support the DOT graph language, such as the [dot command line tool](http://www.graphviz.org/doc/info/command.html) from [graphviz.org](http://www.graphviz.org) or [viz.js](https://github.com/mdaines/viz.js). See http://www.webgraphviz.com for instant gratification.
Command line example: `dot -T pdf -o phoneCall.pdf phoneCall.dot` to generate a PDF file.


##Project Goals

Stateless is a base for exploration of generic and functional programming to drive workflow in .NET.

This page is an almost-complete description of Stateless, and its explicit aim is to remain minimal.

Please use the issue tracker or the if you'd like to report problems or discuss features.

(_Why the name? Stateless implements the set of rules regarding state transitions, but, at least when the delegate version of the constructor is used, doesn't maintain any internal state itself._)
