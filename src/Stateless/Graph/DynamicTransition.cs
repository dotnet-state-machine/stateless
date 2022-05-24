using Stateless.Reflection;

namespace Stateless.Graph;

/// <summary>
///     Represents a dynamic transition.
/// </summary>
public class DynamicTransition : Transition {
    /// <summary>
    ///     The state where this transition finishes
    /// </summary>
    public State DestinationState { get; }

    /// <summary>
    ///     When is this transition followed
    /// </summary>
    public string Criterion { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="DynamicTransition" />.
    /// </summary>
    /// <param name="sourceState">The source state.</param>
    /// <param name="destinationState">The destination state.</param>
    /// <param name="trigger">The trigger associated with this transition.</param>
    /// <param name="criterion">The reason the destination state was chosen.</param>
    public DynamicTransition(State sourceState, State destinationState, TriggerInfo trigger, string criterion)
        : base(sourceState, trigger) {
        DestinationState = destinationState;
        Criterion        = criterion;
    }
}