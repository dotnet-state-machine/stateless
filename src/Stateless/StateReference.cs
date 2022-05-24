namespace Stateless;

internal class StateReference<TState> where TState : notnull {
    public TState State { get; set; }

    public StateReference(TState state) => State = state;
}