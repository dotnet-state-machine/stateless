namespace Stateless;

internal class StateReference<TState> {
    public TState State { get; set; }

    public StateReference(TState state) => State = state;
}