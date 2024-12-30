# Stateless

**Create *state machines* and lightweight *state machine-based workflows* directly in .NET code:**

```csharp
var phoneCall = new StateMachine<State, Trigger>(State.OffHook);

phoneCall.Configure(State.OffHook)
    .Permit(Trigger.CallDialled, State.Ringing);

phoneCall.Configure(State.Connected)
    .OnEntry(t => StartCallTimer())
    .OnExit(t => StopCallTimer())
    .InternalTransition(Trigger.MuteMicrophone, t => OnMute())
    .InternalTransition(Trigger.UnmuteMicrophone, t => OnUnmute())
    .InternalTransition<int>(_setVolumeTrigger, (volume, t) => OnSetVolume(volume))
    .Permit(Trigger.LeftMessage, State.OffHook)
    .Permit(Trigger.PlacedOnHold, State.OnHold);

// ...

phoneCall.Fire(Trigger.CallDialled);
Assert.AreEqual(State.Ringing, phoneCall.State);
```

This project, as well as the example above, was inspired by 
[Simple State Machine (Archived)](https://web.archive.org/web/20170814020207/http://simplestatemachine.codeplex.com/).

## Features

Most standard state machine constructs are supported:

 * Generic support for states and triggers of any .NET type (numbers, strings, enums, etc.)
 * Hierarchical states
 * Entry/exit actions for states
 * Guard clauses to support conditional transitions
 * Introspection

Some useful extensions are also provided:

 * Ability to store state externally (for example, in a property tracked by an ORM)
 * Parameterised triggers
 * Reentrant states
 * Export to DOT and Mermaid graph

## Documentation

For guidance on how to use Stateles, the 
[project README file](https://github.com/dotnet-state-machine/stateless?tab=readme-ov-file#stateless----) 
contains documentation and code examples. The source repository also includes a few 
[example projects](https://github.com/dotnet-state-machine/stateless/tree/dev/example).

## Contributing

We welcome contributions to this project. Check 
[CONTRIBUTING.md](https://github.com/dotnet-state-machine/stateless/blob/dev/CONTRIBUTING.md) for more info.
