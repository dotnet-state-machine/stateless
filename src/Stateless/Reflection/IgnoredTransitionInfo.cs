namespace Stateless.Reflection;

/// <summary>
///     Describes a trigger that is "ignored" (stays in the same state)
/// </summary>
public class IgnoredTransitionInfo : TransitionInfo {
    /// <inheritdoc />
    private IgnoredTransitionInfo(IEnumerable<InvocationInfo> guardConditionsMethodDescriptions, TriggerInfo trigger) :
        base(guardConditionsMethodDescriptions, trigger) { }

    internal static IgnoredTransitionInfo Create<TState, TTrigger>(
        StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour behaviour)
        where TState : notnull where TTrigger : notnull {
        return new IgnoredTransitionInfo(behaviour.Guard.Conditions.Select(c => c.MethodDescription),
                                         new TriggerInfo(behaviour.Trigger));
    }
}