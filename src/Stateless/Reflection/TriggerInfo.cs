namespace Stateless.Reflection;

/// <summary>
///     Represents a trigger in a StateMachine.
/// </summary>
public class TriggerInfo {
    /// <summary>
    ///     The instance or value this trigger represents.
    /// </summary>
    public object? UnderlyingTrigger { get; }

    internal TriggerInfo(object? underlyingTrigger) => UnderlyingTrigger = underlyingTrigger;

    /// <summary>
    ///     Describes the trigger.
    /// </summary>
    public override string ToString() => UnderlyingTrigger?.ToString() ?? "<null>";
}