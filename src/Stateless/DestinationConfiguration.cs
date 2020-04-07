using System;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// 
        /// </summary>
        public class DestinationConfiguration
        {
            private readonly TransitionConfiguration _transitionConfiguration;
            private readonly TState _destination;
            private readonly TriggerBehaviour _triggerBehaviour;

            internal DestinationConfiguration(TransitionConfiguration transitionConfiguration, TState destination, TriggerBehaviour triggerBehaviour)
            {
                _transitionConfiguration = transitionConfiguration;
                _destination = destination;
                _triggerBehaviour = triggerBehaviour;
            }

            internal DestinationConfiguration If(Func<object [], bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(guard, description));
                return this;
            }

            internal StateConfiguration Do(Action<object[], object> someAction)
            {
                _triggerBehaviour.AddAction(someAction);
                return _transitionConfiguration.StateConfiguration;
            }
        }
    }
}