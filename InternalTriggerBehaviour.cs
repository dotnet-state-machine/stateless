using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class InternalTriggerBehaviour : TriggerBehaviour
        {

            public InternalTriggerBehaviour(TTrigger trigger, Func<bool> guard)
                : base(trigger, guard)
            {
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = source;
                return false;
            }
        }
    }
}
