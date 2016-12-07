using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TriggerBehaviourResult
        {
            public TriggerBehaviourResult(TriggerBehaviour handler, ICollection<string> unmetGuards)
            {
                Handler = handler;
                UnmetUnmetGuardConditions = unmetGuards;
            }
            public TriggerBehaviour Handler { get; private set; }
            public ICollection<string> UnmetUnmetGuardConditions { get; private set; }
        }
    }
}
