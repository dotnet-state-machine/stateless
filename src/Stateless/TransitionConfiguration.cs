using System;
using System.Linq;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// 
        /// </summary>
        public class TransitionConfiguration
        {
            private readonly StateConfiguration _stateConfiguration;
            private readonly TTrigger _trigger;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stateConfiguration"></param>
            /// <param name="trigger"></param>
            public TransitionConfiguration(StateConfiguration stateConfiguration, TTrigger trigger)
            {
                _stateConfiguration = stateConfiguration;
                _trigger = trigger;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="destination"></param>
            /// <returns></returns>
            public DestinationConfiguration To(TState destination)
            {
                TriggerBehaviour triggerBehaviour = new TransitioningTriggerBehaviour(_trigger, destination, null);
                _stateConfiguration.AddTriggerBehaviour(triggerBehaviour);
                return new DestinationConfiguration(this, destination, triggerBehaviour);
            }

            internal TriggerBehaviour GetTriggerBehaviour()
            {
                return _stateConfiguration.Representation.TriggerBehaviours[_trigger].First();
            }

            internal DestinationConfiguration Self()
            {
                var destinationState = _stateConfiguration.State;
                var ttb = new TransitioningTriggerBehaviour(_trigger, destinationState, null);
                _stateConfiguration.AddTriggerBehaviour(ttb);
                return new DestinationConfiguration(this, destinationState, ttb);
            }

            internal DestinationConfiguration Internal()
            {
                var destinationState = _stateConfiguration.State;
                var itb = new InternalTriggerBehaviour.Sync(_trigger, (t) => true, (t, r) => { });
                _stateConfiguration.AddTriggerBehaviour(itb);
                return new DestinationConfiguration(this, destinationState, itb);
            }
        }
    }
}