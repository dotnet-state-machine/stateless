namespace Stateless.Reflection;

/// <summary>
///     Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
/// </summary>
[PublicAPI]
public class DynamicTransitionInfo : TransitionInfo {
    /// <summary>
    ///     Gets method information for the destination state selector.
    /// </summary>
    public InvocationInfo DestinationStateSelectorDescription { get; }

    /// <summary>
    ///     Gets the possible destination states.
    /// </summary>
    public DynamicStateInfos? PossibleDestinationStates { get; }

    /// <inheritdoc />
    private DynamicTransitionInfo(IEnumerable<InvocationInfo> guardConditionsMethodDescriptions, TriggerInfo trigger,
                                  InvocationInfo              destinationStateSelectorDescription,
                                  DynamicStateInfos?          possibleDestinationStates) :
        base(guardConditionsMethodDescriptions, trigger) {
        DestinationStateSelectorDescription = destinationStateSelectorDescription;
        PossibleDestinationStates           = possibleDestinationStates;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="DynamicTransitionInfo" />.
    /// </summary>
    /// <typeparam name="TTrigger">The trigger type.</typeparam>
    /// <param name="trigger">The trigger associated with this transition.</param>
    /// <param name="guards">The guard conditions associated with this transition.</param>
    /// <param name="selector">The destination selector associated with this transition.</param>
    /// <param name="possibleStates">The possible destination states.</param>
    /// <returns></returns>
    public static DynamicTransitionInfo Create<TTrigger>(TTrigger       trigger,  IEnumerable<InvocationInfo>? guards,
                                                         InvocationInfo selector, DynamicStateInfos? possibleStates) =>
        new(guards ?? new List<InvocationInfo>(), new TriggerInfo(trigger), selector, possibleStates);
}