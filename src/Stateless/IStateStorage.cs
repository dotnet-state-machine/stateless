namespace Stateless;

/// <summary>
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IStateStorage<TState> {
    /// <summary>
    /// </summary>
    /// <returns></returns>
    TState State { get; set; }
}

internal class InternalStateStorage<TState> : IStateStorage<TState> where TState : notnull {
    public InternalStateStorage(TState state) => State = state;
    public TState State { get; set; }
}

internal class ExternalStateStorage<TState> : IStateStorage<TState> where TState : notnull {
    private readonly Func<TState>   _stateAccessor;
    private readonly Action<TState> _stateMutator;

    public ExternalStateStorage(Func<TState> stateAccessor, Action<TState> stateMutator) {
        _stateAccessor = stateAccessor;
        _stateMutator  = stateMutator;
    }

    public TState State {
        get => _stateAccessor();
        set => _stateMutator(value);
    }
}