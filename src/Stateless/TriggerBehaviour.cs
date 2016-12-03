using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            readonly TTrigger _trigger;
            readonly TransitionGuards _guards;

            protected TriggerBehaviour(TTrigger trigger, TransitionGuards guards)
            {
                _trigger = trigger;
                _guards = guards;

            }

            public TTrigger Trigger { get { return _trigger; } }
            internal ICollection<Func<bool>> Guards { get { return _guards.List.Select(g => g.Guard).ToList(); } }
            internal string GuardsDescriptions
            {
                get
                {
                    var guardsDescriptions = _guards.List
                        .Select(d => d.GuardDescription ?? d.Guard.TryGetMethodName());

                    return string
                        .Join(",", guardsDescriptions);
                }
            }

            public bool IsGuardConditionMet
            {
                get
                {
                    return _guards.List.All(c => c.Guard());
                }
            }
            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}
