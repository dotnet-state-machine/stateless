using System;

namespace Stateless
{
    partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// This class contains the required trigger information for a transition.
        /// </summary>
        public class DestinationConfiguration
        {
            private readonly TransitionConfiguration _transitionConfiguration;
            private readonly TriggerBehaviour _triggerBehaviour;
            private readonly StateRepresentation _representation;
            /// <summary>
            /// 
            /// </summary>
            public StateMachine<TState, TTrigger> Machine { get { return _transitionConfiguration.StateConfiguration.Machine; } }

            internal DestinationConfiguration(TransitionConfiguration transitionConfiguration, TriggerBehaviour triggerBehaviour, StateRepresentation representation)
            {
                _transitionConfiguration = transitionConfiguration;
                _triggerBehaviour = triggerBehaviour;
                _representation = representation;
            }
            /// <summary>
            /// Adds a guard function to the trigger. This guard function will determine if the transition will occur or not.
            /// </summary>
            /// <param name="guard">This method is run when the state machine fires the trigger.</param>
            /// <param name="description">Optional description of the guard</param>
            public DestinationConfiguration If(Func<bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(guard, description));
                return this;
            }

            /// <summary>
            /// Adds a guard function to the trigger. This guard function will determine if the transition will occur or not.
            /// </summary>
            /// <param name="guard">This method is run when the state machine fires the trigger.</param>
            /// <param name="description">Optional description of the guard</param>
            public DestinationConfiguration If(Func<object[], bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(guard, description));
                return this;
            }

            /// <summary>
            /// Adds a guard function to the trigger. This guard function will determine if the transition will occur or not.
            /// </summary>
            /// <typeparam name="TArg">The parameter to the guard function </typeparam>
            /// <param name="guard">This method is run when the state machine fires the trigger.</param>
            /// <param name="description">Optional description of the guard</param>
            public DestinationConfiguration If<TArg>(Func<TArg, bool> guard, string description = null)
            {
                _triggerBehaviour.SetGuard(new TransitionGuard(TransitionGuard.ToPackedGuard(guard), description));
                return this;
            }

            /// <summary>
            /// Creates a new transition. Use To(), Self(), Internal() or Dynamic() to set up the destination.
            /// </summary>
            /// <param name="trigger">The event trigger that will trigger this transition.</param>
            public TransitionConfiguration Transition(TTrigger trigger)
            {
                return new TransitionConfiguration(_transitionConfiguration.StateConfiguration, _representation, trigger);
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            public StateConfiguration Do(Action someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction());
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            public StateConfiguration Do(Action<Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction(someAction);
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <typeparam name="TArg">The parameter used by the action.</typeparam>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            public StateConfiguration Do<TArg>(Action<TArg, Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction(ParameterConversion.Unpack<TArg>(args, 0), t));
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <typeparam name="TArg0">The parameter used by the action.</typeparam>
            /// <typeparam name="TArg1">The parameter used by the action.</typeparam>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            public StateConfiguration Do<TArg0, TArg1>(Action<TArg0, TArg1, Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction(ParameterConversion.Unpack<TArg0>(args, 0), ParameterConversion.Unpack<TArg1>(args, 1), t));
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// Adds an action to a transition. The action will be executed before the Exit action(s) (if any) are executed.
            /// </summary>
            /// <typeparam name="TArg0">The parameter used by the action.</typeparam>
            /// <typeparam name="TArg1">The parameter used by the action.</typeparam>
            /// <typeparam name="TArg2">The parameter used by the action.</typeparam>
            /// <param name="someAction">The action run when the trigger event is handled.</param>
            public StateConfiguration Do<TArg0, TArg1, TArg2>(Action<TArg0, TArg1, TArg2, Transition> someAction)
            {
                if (someAction == null) throw new ArgumentNullException(nameof(someAction));

                _triggerBehaviour.AddAction((t, args) => someAction(ParameterConversion.Unpack<TArg0>(args, 0), ParameterConversion.Unpack<TArg1>(args, 1), ParameterConversion.Unpack<TArg2>(args, 2), t));
                return _transitionConfiguration.StateConfiguration;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="action"></param>
            /// <returns></returns>
            public StateConfiguration OnExit(Action action)
            {
                return _transitionConfiguration.OnExit(action);
            }
        }
    }
}