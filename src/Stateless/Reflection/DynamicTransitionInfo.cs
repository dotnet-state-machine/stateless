#nullable enable

using System.Collections.Generic;

namespace Stateless.Reflection; 

/// <summary>
/// 
/// </summary>
public class DynamicStateInfo
{
    /// <summary>
    /// Constructor
    /// </summary>
    public DynamicStateInfo(string destinationState, string criterion)
    {
        DestinationState = destinationState;
        Criterion        = criterion;
    }

    /// <summary>
    /// The name of the destination state
    /// </summary>
    public string DestinationState { get; }

    /// <summary>
    /// The reason this destination state was chosen
    /// </summary>
    public string Criterion { get; }
}

/// <summary>
/// List of DynamicStateInfo objects, with "add" function for ease of definition
/// </summary>
public class DynamicStateInfos : List<DynamicStateInfo>
{
    /// <summary>
    /// Add a DynamicStateInfo with less typing
    /// </summary>
    /// <param name="destinationState"></param>
    /// <param name="criterion"></param>
    public void Add(string destinationState, string criterion)
    {
        base.Add(new DynamicStateInfo(destinationState, criterion));
    }

    /// <summary>
    /// Add a DynamicStateInfo with less typing
    /// </summary>
    /// <param name="destinationState"></param>
    /// <param name="criterion"></param>
    public void Add<TState>(TState destinationState, string criterion) where TState : notnull
    {
        base.Add(new DynamicStateInfo(destinationState.ToString()!, criterion));
    }
}

/// <summary>
/// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
/// </summary>
public class DynamicTransitionInfo : TransitionInfo
{
    /// <summary>
    /// Gets method information for the destination state selector.
    /// </summary>
    public InvocationInfo DestinationStateSelectorDescription { get; }

    /// <summary>
    /// Gets the possible destination states.
    /// </summary>
    public DynamicStateInfos PossibleDestinationStates { get; }

    /// <summary>
    /// Creates a new instance of <see cref="DynamicTransitionInfo"/>.
    /// </summary>
    /// <typeparam name="TTrigger">The trigger type.</typeparam>
    /// <param name="trigger">The trigger associated with this transition.</param>
    /// <param name="guards">The guard conditions associated with this transition.</param>
    /// <param name="selector">The destination selector associated with this transition.</param>
    /// <param name="possibleStates">The possible destination states.</param>
    /// <returns></returns>
    public static DynamicTransitionInfo Create<TTrigger>(TTrigger       trigger,  IEnumerable<InvocationInfo>? guards,
                                                         InvocationInfo selector, DynamicStateInfos possibleStates)
    {
        return new DynamicTransitionInfo(guards ?? new List<InvocationInfo>(), new TriggerInfo(trigger), selector, possibleStates);
    }

    /// <inheritdoc />
    private DynamicTransitionInfo(IEnumerable<InvocationInfo> guardConditionsMethodDescriptions, TriggerInfo trigger,
                                  InvocationInfo destinationStateSelectorDescription,
                                  DynamicStateInfos possibleDestinationStates) : base(guardConditionsMethodDescriptions,
     trigger) {
        DestinationStateSelectorDescription = destinationStateSelectorDescription;
        PossibleDestinationStates           = possibleDestinationStates;
    }
}