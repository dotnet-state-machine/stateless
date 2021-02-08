using System;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// This class contains the required information for a transition.
        /// </summary>
        public class TransitionConfiguration
        {
            internal StateConfiguration StateConfiguration { get; private set; }

            private readonly StateRepresentation _representation;
            private readonly TTrigger _trigger;

            /// <summary>
            /// The TransitionConfiguration contains the information required to create a new transition.
            /// </summary>
            /// <param name="stateConfiguration"> Current state being configured</param>
            /// <param name="representation">A representation of the state</param>
            /// <param name="trigger">Trigger for this transition</param>
            internal TransitionConfiguration(StateConfiguration stateConfiguration, StateRepresentation representation, TTrigger trigger)
            {
                StateConfiguration = stateConfiguration;
                _representation = representation;
                _trigger = trigger;
            }

            /// <summary>
            /// Adds a new transition to the destination state
            /// </summary>
            /// <param name="destination">Destination state</param>
            public DestinationConfiguration To(TState destination)
            {
                TriggerBehaviour triggerBehaviour = new TransitioningTriggerBehaviour(_trigger, destination, null);
                _representation.AddTriggerBehaviour(triggerBehaviour);
                return new DestinationConfiguration(this, triggerBehaviour, _representation);
            }

            /// <summary>
            /// Creates a new re-entrant transition. This transition, when triggered, will execute the state's
            /// Exit and Entry actions, if any has been added.
            /// </summary>
            public DestinationConfiguration Self()
            {
                var destinationState = StateConfiguration.State;
                var ttb = new ReentryTriggerBehaviour(_trigger, destinationState, null);
                _representation.AddTriggerBehaviour(ttb);
                return new DestinationConfiguration(this, ttb, _representation);
            }

            /// <summary>
            /// Creates a new internal transition. This transition, when triggered, not cause any state change,
            /// nor will the Exit or Entry actions of the state be executed. Only the action configured will be
            /// executed.
            /// </summary>
            /// <returns></returns>
            public DestinationConfiguration Internal()
            {
                var destinationState = StateConfiguration.State;
                var itb = new InternalTriggerBehaviour.Sync(_trigger, (t) => true, (t, r) => { });
                _representation.AddTriggerBehaviour(itb);
                return new DestinationConfiguration(this, itb, _representation);
            }

            /// <summary>
            /// Creates a new dynamic transition. The destination is determined at run time. A Func must be
            /// supplied, this method will determine the destination state.
            /// </summary>
            /// <param name="destinationStateSelector">A method to determine the destination state</param>
            /// <param name="destinationStateSelectorDescription">A description of the state selector</param>
            /// <param name="possibleDestinationStates">An optional list of states (useful if the DotGraph feature is used).</param>
            public DestinationConfiguration Dynamic(Func<TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
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

                return new DestinationConfiguration(this, dtb, _representation);
            }

            /// <summary>
            /// Creates a new dynamic transition. The destination is determined at run time. A Func must be
            /// supplied, this method will determine the destination state.
            /// </summary>
            /// <typeparam name="TArg">A parameter for the destination selector Func.</typeparam>
            /// <param name="destinationStateSelector">A method to determine the destination state</param>
            /// <param name="destinationStateSelectorDescription">A description of the state selector</param>
            /// <param name="possibleDestinationStates">An optional list of states (useful if the DotGraph feature is used).</param>
            public DestinationConfiguration Dynamic<TArg>(Func<TArg, TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
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

                return new DestinationConfiguration(this, dtb, _representation);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="exitAction"></param>
            /// <returns></returns>
            public StateConfiguration OnExit(Action exitAction)
            {
                return StateConfiguration.OnExit(exitAction);
            }

            /// <summary>
            /// Specify an action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="exitAction">Action to execute, providing details of the transition.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExit(Action<Transition> exitAction, string exitActionDescription = null)
            {
                return StateConfiguration.OnExit(exitAction, exitActionDescription);
            }

            /// <summary>
            /// Ignore the specified trigger when in the configured state.
            /// </summary>
            /// <param name="trigger">The trigger to ignore.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration Ignore(TTrigger trigger)
            {
                return StateConfiguration.Ignore(trigger);
            }
        }
    }
}