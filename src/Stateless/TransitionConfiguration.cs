using System;

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
                _stateConfiguration.Representation.AddTriggerBehaviour(new TransitioningTriggerBehaviour(_trigger, destination, null));
                return new DestinationConfiguration(this, destination);
            }
        }
    }
}