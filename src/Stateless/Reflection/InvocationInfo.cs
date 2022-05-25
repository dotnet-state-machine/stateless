namespace Stateless.Reflection;

/// <summary>
///     Describes a method - either an action (activate, deactivate, etc.) or a transition guard
/// </summary>
[PublicAPI]
public class InvocationInfo {
    /// <summary>
    ///     Is the method synchronous or asynchronous?
    /// </summary>
    public enum Timing {
        /// <summary>Method is synchronous</summary>
        Synchronous,

        /// <summary>Method is asynchronous</summary>
        Asynchronous
    }

    /// <summary>
    ///     Text returned for compiler-generated functions where the caller has not specified a description
    /// </summary>
    public const string DefaultFunctionDescription = "Function";

    private readonly string? _description;

    private readonly Timing _timing;

    /// <summary>
    ///     The name of the invoked method.  If the method is a lambda or delegate, the name will be a compiler-generated
    ///     name that is often not human-friendly (e.g. "(.ctor)b__2_0" except with angle brackets instead of parentheses)
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    ///     A description of the invoked method.  Returns:
    ///     1) The user-specified description, if any
    ///     2) else if the method name is compiler-generated, returns DefaultFunctionDescription
    ///     3) else the method name
    /// </summary>
    public string Description {
        get {
            if (_description is { })
                return _description;
            if (MethodName is null)
                return "<null>";
            if (MethodName.IndexOfAny(new[] { '<', '>', '`' }) >= 0)
                return DefaultFunctionDescription;
            return MethodName;
        }
    }

    /// <summary>
    ///     Returns true if the method is invoked asynchronously.
    /// </summary>
    public bool IsAsync => _timing == Timing.Asynchronous;

    /// <summary>
    ///     Creates a new instance of <see cref="InvocationInfo" />.
    /// </summary>
    /// <param name="methodName">The name of the invoked method.</param>
    /// <param name="description">A description of the invoked method.</param>
    /// <param name="timing">Sets a value indicating whether the method is invoked asynchronously.</param>
    private InvocationInfo(string? methodName, string? description, Timing timing) {
        MethodName   = methodName;
        _description = description;
        _timing      = timing;
    }

    internal static InvocationInfo Create(Delegate? method, string? description, Timing timing = Timing.Synchronous) =>
        new(method?.TryGetMethodName(), description, timing);
}