using System;
using System.Threading.Tasks;

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

            internal DestinationConfiguration If<TArg0, TArg1>(Func<TArg0, TArg1, bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(TransitionGuard.ToPackedGuard(guard), description));
                return this;
            }

            internal DestinationConfiguration If<TArg0, TArg1, TArg2>(Func<TArg0, TArg1, TArg2, bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(TransitionGuard.ToPackedGuard(guard), description));
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