using Stateless.Reflection;

namespace Stateless.Graph;

/// <summary>
///     Represents a transition from a state to itself.
/// </summary>
public class StayTransition : Transition {
    /// <summary>
    ///     The guard conditions associated with this transition.
    /// </summary>
    public IEnumerable<InvocationInfo> Guards { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="StayTransition" />.
    /// </summary>
    /// <param name="sourceState">The source state.</param>
    /// <param name="trigger">The trigger associated with this transition.</param>
    /// <param name="guards">The guard conditions associated with this transition.</param>
    /// <param name="executeEntryExitActions">
    ///     Sets whether the entry and exit actions are executed when the transition is
    ///     enacted.
    /// </param>
    public StayTransition(State sourceState, TriggerInfo trigger, IEnumerable<InvocationInfo> guards,
                          bool  executeEntryExitActions)
        : base(sourceState, trigger) {
        ExecuteEntryExitActions = executeEntryExitActions;
        Guards                  = guards;
    }
}