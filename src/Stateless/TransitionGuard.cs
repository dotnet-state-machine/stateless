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

            public static readonly TransitionGuard Empty = new TransitionGuard(new Tuple<Func<object[],bool>, string>[0]);

            #region Generic TArg0, ... to object[] converters

            public static Func<object[], bool> ToPackedGuard<TArg0>(Func<TArg0, bool> guard)
            {
                return args => guard(ParameterConversion.Unpack<TArg0>(args, 0));
            }

            public static Func<object[], bool> ToPackedGuard<TArg0, TArg1>(Func<TArg0, TArg1, bool> guard)
            {
                return args => guard(
                    ParameterConversion.Unpack<TArg0>(args, 0), 
                    ParameterConversion.Unpack<TArg1>(args, 1));
            }

            public static Func<object[], bool> ToPackedGuard<TArg0, TArg1, TArg2>(Func<TArg0, TArg1, TArg2, bool> guard)
            {
                return args => guard(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2));
            }

            public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0>(Tuple<Func<TArg0, bool>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0, TArg1>(Tuple<Func<TArg0, TArg1, bool>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0, TArg1, TArg2>(Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            #endregion

            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2)))
                    .ToList();
            }

            internal TransitionGuard(Func<bool> guard, string description = null)
            {
                Conditions = new List<GuardCondition>
                {
                    new GuardCondition(guard, Reflection.InvocationInfo.Create(guard, description))
                };
            }

            internal TransitionGuard(Tuple<Func<object[], bool>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2)))
                    .ToList();
            }

            internal TransitionGuard(Func<object[], bool> guard, string description = null)
            {
                Conditions = new List<GuardCondition>
                {
                    new GuardCondition(guard, Reflection.InvocationInfo.Create(guard, description))
                };
            }
            
            /// <summary>
            /// Guards is the list of the guard functions for all guard conditions for this transition
            /// </summary>
            internal ICollection<Func<object[], bool>> Guards => Conditions.Select(g => g.Guard).ToList();

            /// <summary>
            /// GuardConditionsMet is true if all of the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public bool GuardConditionsMet(object[] args)
            {
                return Conditions.All(c => c.Guard == null || c.Guard(args));
            }

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public ICollection<string> UnmetGuardConditions(object[] args)
            {
                return Conditions
                    .Where(c => !c.Guard(args))
                    .Select(c => c.Description)
                    .ToList();
            }
        }
    }
}