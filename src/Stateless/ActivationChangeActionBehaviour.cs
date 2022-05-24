using Stateless.Reflection;

namespace Stateless;

internal sealed class ActivationChangeActionBehaviour {
    private readonly EventCallback _callback = EventCallbackFactory.Empty;

    internal InvocationInfo Description { get; }

    public ActivationChangeActionBehaviour(Action action, InvocationInfo actionDescription)
        : this(actionDescription) =>
        _callback = EventCallbackFactory.Create(action);

    public ActivationChangeActionBehaviour(Func<Task> action, InvocationInfo actionDescription)
        : this(actionDescription) =>
        _callback = EventCallbackFactory.Create(action);

    private ActivationChangeActionBehaviour(InvocationInfo actionDescription) =>
        Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));

    public Task ExecuteAsync() => _callback.InvokeAsync();
}