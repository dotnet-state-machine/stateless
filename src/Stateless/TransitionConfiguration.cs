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
            internal StateConfiguration StateConfiguration { get; private set; }

            private readonly StateRepresentation _representation;
            private readonly TTrigger _trigger;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stateConfiguration"></param>
            /// <param name="trigger"></param>
            internal TransitionConfiguration(StateConfiguration stateConfiguration, StateRepresentation representation ,TTrigger trigger)
            {
                StateConfiguration = stateConfiguration;
                _representation = representation;
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
                _representation.AddTriggerBehaviour(triggerBehaviour);
                return new DestinationConfiguration(this, triggerBehaviour);
            }

            internal DestinationConfiguration Self()
            {
                var destinationState = StateConfiguration.State;
                var ttb = new TransitioningTriggerBehaviour(_trigger, destinationState, null);
                _representation.AddTriggerBehaviour(ttb);
                return new DestinationConfiguration(this, ttb);
            }

            internal DestinationConfiguration Internal()
            {
                var destinationState = StateConfiguration.State;
                var itb = new InternalTriggerBehaviour.Sync(_trigger, (t) => true, (t, r) => { });
                _representation.AddTriggerBehaviour(itb);
                return new DestinationConfiguration(this, itb);
            }

            internal DestinationConfiguration Dynamic(Func<TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                var dtb = new DynamicTriggerBehaviour(_trigger,
                        args => destinationStateSelector(),
                        null,           // No transition guard
                        Reflection.DynamicTransitionInfo.Create(_trigger,
                            null,       // No guards
                            Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                            possibleDestinationStates
                        )
                    );

                _representation.AddTriggerBehaviour(dtb);

                return new DestinationConfiguration(this, dtb);
            }

            internal DestinationConfiguration Dynamic<TArg>(Func<TArg, TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                var dtb = new DynamicTriggerBehaviour(_trigger,
                        args => destinationStateSelector(ParameterConversion.Unpack<TArg>(args, 0)),
                        null,           // No transition guard
                        Reflection.DynamicTransitionInfo.Create(_trigger,
                            null,       // No guards
                            Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                            possibleDestinationStates
                        )
                    );

                _representation.AddTriggerBehaviour(dtb);

                return new DestinationConfiguration(this, dtb);

            }
        }
    }
}