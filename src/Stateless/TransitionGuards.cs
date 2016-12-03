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
        public class TransitionGuard
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransitionGuard"/> class.
            /// </summary>
            /// <param name="guard">The guard.</param>
            /// <param name="guardDescription">The guard description.</param>
            public TransitionGuard(Func<bool> guard, string guardDescription)
            {
                _guard = guard;
                _guardDescription = guardDescription;
            }
            readonly Func<bool> _guard;
            readonly string _guardDescription;
            internal Func<bool> Guard { get { return _guard; } }
            internal string GuardDescription { get { return _guardDescription; } }
        }

        /// <summary>
        /// List of guards.
        /// </summary>
        public class TransitionGuards
        {
            readonly IList<TransitionGuard> _guardsList;

            internal TransitionGuards(Func<bool> guard = null, string guardDescription = null)
            {
                _guardsList = new List<TransitionGuard>();

                if (guard != null)
                    And(guard, guardDescription);
            }
            /// <summary>
            /// Add guard to list of guards.
            /// </summary>
            /// <param name="guard">The guard.</param>
            /// <param name="guardDescription">The guard description.</param>
            /// <returns></returns>
            public TransitionGuards And(Func<bool> guard, string guardDescription = null)
            {
                _guardsList.Add(new TransitionGuard(guard, guardDescription));
                return this;
            }
        }
    }
}