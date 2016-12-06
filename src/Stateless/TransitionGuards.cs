using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// Guard function and description.
        /// </summary>
        public class GuardCondition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransitionGuard"/> class.
            /// </summary>
            /// <param name="guard">The guard.</param>
            /// <param name="guardDescription">The guard description.</param>
            public GuardCondition(Func<bool> guard, string guardDescription)
            {
                _guard = guard;
                _guardDescription = guardDescription;
            }
            readonly Func<bool> _guard;
            readonly string _guardDescription;
            internal Func<bool> Guard { get { return _guard; } }
            internal string GuardDescription { get { return _guardDescription ?? _guard.TryGetMethodName(); } }
        }

        /// <summary>
        /// List of guards.
        /// </summary>
        public class TransitionGuard
        {
            /// <summary>
            /// List of added guards
            /// </summary>
            public IList<GuardCondition> Conditions { get; private set; }

            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, g.Item2))
                    .ToList();
            }

            internal TransitionGuard(Func<bool> guard = null, string guardDescription = null)
            {
                Conditions = new List<GuardCondition> { new GuardCondition(guard, guardDescription) };
            }
        }
    }
}