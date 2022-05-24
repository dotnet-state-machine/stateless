using Stateless.Reflection;

namespace Stateless.Graph;

/// <summary>
///     Used to keep track of transitions between states
/// </summary>
[PublicAPI]
public abstract class Transition {
    /// <summary>
    ///     The trigger that causes this transition
    /// </summary>
    public TriggerInfo Trigger { get; }

    /// <summary>
    ///     List of actions to be performed by the destination state (the one being entered)
    /// </summary>
    public List<ActionInfo> DestinationEntryActions { get; } = new();

    /// <summary>
    ///     Should the entry and exit actions be executed when this transition takes place
    /// </summary>
    public bool ExecuteEntryExitActions { get; protected set; } = true;

    /// <summary>
    ///     The state where this transition starts
    /// </summary>
    public State SourceState { get; }

    /// <summary>
    ///     Base class of transitions
    /// </summary>
    /// <param name="sourceState"></param>
    /// <param name="trigger"></param>
    protected Transition(State sourceState, TriggerInfo trigger) {
        SourceState = sourceState;
        Trigger     = trigger;
    }
}