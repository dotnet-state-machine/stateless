namespace Stateless.Reflection;

/// <summary>
/// </summary>
public abstract class TransitionInfo {
    /// <summary>
    ///     Method descriptions of the guard conditions.
    ///     Returns a non-null but empty list if there are no guard conditions
    /// </summary>
    public readonly IEnumerable<InvocationInfo> GuardConditionsMethodDescriptions;

    /// <summary>
    ///     The trigger whose firing resulted in this transition.
    /// </summary>
    public TriggerInfo Trigger { get; }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="guardConditionsMethodDescriptions"></param>
    /// <param name="trigger"></param>
    protected TransitionInfo(IEnumerable<InvocationInfo> guardConditionsMethodDescriptions, TriggerInfo trigger) {
        GuardConditionsMethodDescriptions = guardConditionsMethodDescriptions;
        Trigger                           = trigger;
    }
}