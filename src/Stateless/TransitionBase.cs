namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    { 
        /// <summary>
        /// Abstract class for all Transition classes
        /// </summary>
    public abstract class TransitionBase
        {
            /// <summary>
            /// The state transitioned from.
            /// </summary>
            public TState Source { get; internal set; }

            /// <summary>
            /// The state transitioned to.
            /// </summary>
            public TState Destination { get; internal set; }

            /// <summary>
            /// The trigger that caused the transition.
            /// </summary>
            public TTrigger Trigger { get; internal set; }
        }

    }

}