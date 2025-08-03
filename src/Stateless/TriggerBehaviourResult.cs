using System.Collections.Generic;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TriggerBehaviourResult
        {
            public TriggerBehaviourResult(TriggerBehaviour handler, ICollection<string> unmetGuardConditions)
            {
                Handler = handler;
                UnmetGuardConditions = unmetGuardConditions;
            }
            public TriggerBehaviourResult(TriggerBehaviourAsync handler, ICollection<string> unmetGuardConditions)
            {
                HandlerAsync = handler;
                UnmetGuardConditions = unmetGuardConditions;
            }
            public TriggerBehaviour Handler { get; }
            public TriggerBehaviourAsync HandlerAsync { get; }
            public ICollection<string> UnmetGuardConditions { get; }
        }
    }
}
