using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Decoration
{
    /// <summary>
    /// Decorates a StateMachine to keep an audit trail of all transistions.
    /// </summary>
    /// <typeparam name="TState">The type used to represent the states.</typeparam>
    /// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
    public class AuditTrailDecorator<TState, TTrigger> : StateMachineDecoratorBase<TState, TTrigger>
    {
        /// <summary>
        /// Class to hold information about a transistion in the audit trail.
        /// </summary>
        public class TransistionInfo
        {
            private readonly DateTime _dateTime;
            private readonly StateMachine<TState, TTrigger>.Transition _transition;

            /// <summary>
            /// Construct a TransitionInfo.
            /// </summary>
            /// <param name="transition">The transistion to wrap.</param>
            public TransistionInfo(StateMachine<TState, TTrigger>.Transition transition)
            {
                _dateTime = DateTime.Now;
                _transition = transition;
            }

            /// <summary>
            /// The moment in time the transistion occurred.
            /// </summary>
            public DateTime DateTime
            {
                get { return _dateTime; }
            }

            /// <summary>
            /// The transistion.
            /// </summary>
            public StateMachine<TState, TTrigger>.Transition Transition
            {
                get { return _transition; }
            }
        }

        private readonly List<TransistionInfo> _auditTrail = new List<TransistionInfo>();
        private readonly int _maximumAuditTrailSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stateMachine">The StateMachine to decorate.</param>
        /// <param name="maximumAuditTrailSize">The maximum number of transistions to keep in the audit trail. First-In-First-Out behaviour.</param>
        public AuditTrailDecorator(IStateMachine<TState, TTrigger> stateMachine, int maximumAuditTrailSize = 0)
        : base(stateMachine)
        {
            stateMachine.OnTransitioned(AddToAuditTrail);

            if (maximumAuditTrailSize < 0) throw new ArgumentException(nameof(maximumAuditTrailSize));
            _maximumAuditTrailSize = maximumAuditTrailSize;
        }

        /// <summary>
        /// The trail of transistions that the StateMachine went through.
        /// The latest entry is the most recent transition that happened.
        /// </summary>
        public IEnumerable<TransistionInfo> AuditTrail => _auditTrail;

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransistionHandler">The action to execute, accepting the details
        /// of the transition.</param>
        public override void OnTransitioned(Action<StateMachine<TState, TTrigger>.Transition> onTransistionHandler)
        {
            base.OnTransitioned((transition) =>
            {
                AddToAuditTrail(transition);
                onTransistionHandler(transition);
            });
        }

        private void AddToAuditTrail(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (_maximumAuditTrailSize > 0 && _auditTrail.Count == _maximumAuditTrailSize)
            {
                _auditTrail.RemoveAt(0);
            }
            var transitionInfo = new TransistionInfo(transition);
            _auditTrail.Add(transitionInfo);
        }
    }
}
