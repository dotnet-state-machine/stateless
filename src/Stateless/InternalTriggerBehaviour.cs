﻿namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal sealed class InternalTriggerBehaviour : TriggerBehaviour {
        private readonly EventCallback<Transition, object?[]> _callback =
            EventCallbackFactory.Create(new Action<Transition, object?[]>(delegate { }));

        public InternalTriggerBehaviour(TTrigger                      trigger, TransitionGuard? guard,
                                        Action<Transition, object?[]> internalAction)
            : this(trigger, guard) =>
            _callback = EventCallbackFactory.Create(internalAction);

        public InternalTriggerBehaviour(TTrigger                          trigger, Func<bool> guard,
                                        Func<Transition, object?[], Task> internalAction,
                                        string?                           guardDescription = null)
            : this(trigger, new TransitionGuard(guard, guardDescription)) =>
            _callback = EventCallbackFactory.Create(internalAction);

        private InternalTriggerBehaviour(TTrigger trigger, TransitionGuard? guard) : base(trigger, guard) { }

        public override bool ResultsInTransitionFrom(TState source, object?[] args, out TState destination) {
            destination = source;
            return false;
        }

        public Task ExecuteAsync(Transition transition, object?[] args) => _callback.InvokeAsync(transition, args);
    }
}