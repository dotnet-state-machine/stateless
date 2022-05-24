namespace Stateless.Reflection;

/// <summary>
/// </summary>
public class DynamicStateInfo {
    /// <summary>
    ///     The name of the destination state
    /// </summary>
    public string DestinationState { get; }

    /// <summary>
    ///     The reason this destination state was chosen
    /// </summary>
    public string Criterion { get; }

    /// <summary>
    ///     Constructor
    /// </summary>
    public DynamicStateInfo(string destinationState, string criterion) {
        DestinationState = destinationState;
        Criterion        = criterion;
    }
}