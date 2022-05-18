using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection; 

/// <summary>
/// Describes a transition that can be initiated from a trigger.
/// </summary>
public class FixedTransitionInfo : TransitionInfo
{
    internal static FixedTransitionInfo Create<TState, TTrigger>(StateMachine<TState, TTrigger>.TriggerBehaviour behaviour, StateInfo destinationStateInfo)  where TState : notnull where TTrigger : notnull
    {
        return new FixedTransitionInfo(behaviour.Guard?.Conditions.Select(c => c.MethodDescription) ?? new List<InvocationInfo>(), new TriggerInfo(behaviour.Trigger), destinationStateInfo);
    }

    /// <inheritdoc />
    private FixedTransitionInfo(IEnumerable<InvocationInfo> guardConditionsMethodDescriptions, TriggerInfo trigger, StateInfo destinationState) : base(guardConditionsMethodDescriptions, trigger) => DestinationState = destinationState;

    /// <summary>
    /// The state that will be transitioned into on activation.
    /// </summary>
    public StateInfo DestinationState { get; }
}