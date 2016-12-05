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
            public TriggerBehaviour Handler { get; set; }
            public ICollection<string> UnmetUnmetGuardConditions { get; set; }
        }
    }
}
