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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="transitionConfiguration"></param>
            /// <param name="destination"></param>
            public DestinationConfiguration(TransitionConfiguration transitionConfiguration, TState destination)
            {
                _transitionConfiguration = transitionConfiguration;
                _destination = destination;
            }
        }
    }
}