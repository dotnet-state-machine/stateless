namespace Stateless.Reflection;

/// <summary>
///     List of DynamicStateInfo objects, with "add" function for ease of definition
/// </summary>
[PublicAPI]
public class DynamicStateInfos : List<DynamicStateInfo> {
    /// <summary>
    ///     Add a DynamicStateInfo with less typing
    /// </summary>
    /// <param name="destinationState"></param>
    /// <param name="criterion"></param>
    public void Add(string destinationState, string criterion) {
        Add(new DynamicStateInfo(destinationState, criterion));
    }

    /// <summary>
    ///     Add a DynamicStateInfo with less typing
    /// </summary>
    /// <param name="destinationState"></param>
    /// <param name="criterion"></param>
    public void Add<TState>(TState destinationState, string criterion) where TState : notnull {
        Add(new DynamicStateInfo(destinationState.ToString()!, criterion));
    }
}