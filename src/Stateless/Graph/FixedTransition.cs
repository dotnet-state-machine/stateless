using Stateless.Reflection;

namespace Stateless.Graph;

/// <summary>
///     Represents a fixed transition.
/// </summary>
public class FixedTransition : Transition {
    /// <summary>
    ///     The state where this transition finishes
    /// </summary>
    public State DestinationState { get; }

    /// <summary>
    ///     Guard functions for this transition (null if none)
    /// </summary>
    public IEnumerable<InvocationInfo> Guards { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="FixedTransition" />.
    /// </summary>
    /// <param name="sourceState">The source state.</param>
    /// <param name="destinationState">The destination state.</param>
    /// <param name="trigger">The trigger associated with this transition.</param>
    /// <param name="guards">The guard conditions associated with this transition.</param>
    public FixedTransition(State                       sourceState, State destinationState, TriggerInfo trigger,
                           IEnumerable<InvocationInfo> guards)
        : base(sourceState, trigger) {
        DestinationState = destinationState;
        Guards           = guards;
    }
}