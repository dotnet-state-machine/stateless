using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition : MethodDescription
        {
            internal GuardCondition(Func<bool> guard, string description)
                : base(description)
            {
                Guard = Enforce.ArgumentNotNull(guard, nameof(guard));
            }
            internal Func<bool> Guard { get; }
            internal override string MethodName { get { return Guard.TryGetMethodName(); } }
            internal override bool IsAsync { get { return false; } }
        }
    }
}