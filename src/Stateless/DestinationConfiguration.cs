using System;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        public class DestinationConfiguration
        {
            private readonly TransitionConfiguration _transitionConfiguration;
            private readonly TriggerBehaviour _triggerBehaviour;

            internal DestinationConfiguration(TransitionConfiguration transitionConfiguration, TriggerBehaviour triggerBehaviour)
            {
                _transitionConfiguration = transitionConfiguration;
                _triggerBehaviour = triggerBehaviour;
            }

            internal DestinationConfiguration If(Func<object[], bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(guard, description));
                return this;
            }

            internal DestinationConfiguration If<TArg0>(Func<TArg0, bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(TransitionGuard.ToPackedGuard(guard), description));
                return this;
            }

            internal StateConfiguration Do(Action someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction());
                return _transitionConfiguration.StateConfiguration;
            }

            internal StateConfiguration Do(Action<Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction(someAction);
                return _transitionConfiguration.StateConfiguration;
            }

            internal StateConfiguration Do<TArg0>(Action<TArg0, Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));
                
                _triggerBehaviour.AddAction((t, args) => someAction(ParameterConversion.Unpack<TArg0>(args, 0), t));
                return _transitionConfiguration.StateConfiguration;
            }
        }
    }
}