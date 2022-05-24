namespace Stateless.Tests;

internal static class InternalExtensionMethods {
    public static bool Includes<TState, TTrigger>(
        this StateMachine<TState, TTrigger>.StateRepresentation representation, TState state) {
        return representation.UnderlyingState.Equals(state) ||
               representation.GetSubstates().Any(s => s.Includes(state));
    }

    internal static bool GuardConditionsMet<TState, TTrigger>(
        this StateMachine<TState, TTrigger>.TriggerBehaviour transitioning) {
        return transitioning.Guard.Conditions.All(c => c.Guard(Array.Empty<object>()));
    }
}