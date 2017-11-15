using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitionGuard
        {
            internal IList<GuardCondition> Conditions { get; }

            public static readonly TransitionGuard Empty = new TransitionGuard(new Tuple<Func<bool>, string>[0]);
            private TriggerWithParameters _trigger;

            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2)))
                    .ToList();
            }

            internal TransitionGuard(Func<bool> guard, string description = null)
            {
                Conditions = new List<GuardCondition> { new GuardCondition(guard, Reflection.InvocationInfo.Create(guard, description)) };
            }
           
            internal TransitionGuard(Tuple<Func<object,bool>, string>[] guards, TriggerWithParameters trigger)
            {
                    _trigger = trigger;
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2)))
                    .ToList();
            }

            internal TransitionGuard(Func<object, bool> guard, TriggerWithParameters trigger, string description = null)
            {
                _trigger = trigger;

                Conditions = new List<GuardCondition> { new GuardCondition(guard, Reflection.InvocationInfo.Create(guard, description)) };
            }
            /// <summary>
            /// Guards is the list of the guard functions for all guard conditions for this transition
            /// </summary>
            internal ICollection<Func<bool>> Guards => Conditions.Select(g => g.Guard).ToList();

            /// <summary>
            /// GuardConditionsMet is true if all of the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public bool GuardConditionsMet => Conditions.All(c => (c.Guard == null || c.Guard()) &&
                (c.ParamitrizedGuard == null || c.ParamitrizedGuard(_trigger.Args.FirstOrDefault())));

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public ICollection<string> UnmetGuardConditions
            {
                get
                {
                    var guardConditions = Conditions.Where(c => c.Guard != null && !c.Guard());
                    var paramitrizedGuard =
                        Conditions.Where(
                            c => c.ParamitrizedGuard != null && !c.ParamitrizedGuard(_trigger.Args.FirstOrDefault()));

                    return guardConditions.Union(paramitrizedGuard)
                        .Select(c => c.Description)
                        .ToList();
                }
            }
        }
    }
}