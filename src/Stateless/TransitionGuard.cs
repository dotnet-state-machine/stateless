using System;
using System.Collections.Generic;
using System.Linq;
using Stateless.Reflection;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal class TransitionGuard
    {
        internal IList<GuardCondition> Conditions { get; }

        public static readonly TransitionGuard Empty = new(ArrayHelper.Empty<Tuple<Func<object[], bool>, string>>());

        #region Generic TArg0, ... to object[] converters

        public static Func<object[], bool> ToPackedGuard<TArg0>(Func<TArg0?, bool> guard)
        {
            return args =>
            {
                try
                {
                    return ParameterConversion.TryUnpack<TArg0>(args, 0, out var arg) && guard(arg);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            };
        }

        public static Func<object[], bool> ToPackedGuard<TArg0, TArg1>(Func<TArg0?, TArg1?, bool> guard)
        {
            return args =>
            {
                try
                {
                    return ParameterConversion.TryUnpack<TArg0>(args, 0, out var arg0)
                        && ParameterConversion.TryUnpack<TArg1>(args, 1, out var arg1)
                        && guard(arg0, arg1);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            };
        }

        public static Func<object[], bool> ToPackedGuard<TArg0, TArg1, TArg2>(Func<TArg0?, TArg1?, TArg2?, bool> guard)
        {
            return args =>
            {
                try
                {
                    return ParameterConversion.TryUnpack<TArg0>(args, 0, out var arg0)
                        && ParameterConversion.TryUnpack<TArg1>(args, 1, out var arg1)
                        && ParameterConversion.TryUnpack<TArg2>(args, 2, out var arg2)
                        && guard(arg0, arg1, arg2);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            };
        }

        public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0>(IEnumerable<Tuple<Func<TArg0?, bool>, string>> guards)
        {
            return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                                  ToPackedGuard(guard.Item1), guard.Item2))
                         .ToArray();
        }

        public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0, TArg1>(IEnumerable<Tuple<Func<TArg0?, TArg1?, bool>, string>> guards)
        {
            return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                                  ToPackedGuard(guard.Item1), guard.Item2))
                         .ToArray();
        }

        public static Tuple<Func<object[], bool>, string>[] ToPackedGuards<TArg0, TArg1, TArg2>(IEnumerable<Tuple<Func<TArg0?, TArg1?, TArg2?, bool>, string>> guards)
        {
            return guards.Select(guard => new Tuple<Func<object[], bool>, string>(
                                  ToPackedGuard(guard.Item1), guard.Item2))
                         .ToArray();
        }

        #endregion

        internal TransitionGuard(IEnumerable<Tuple<Func<bool>, string>> guards)
        {
            Conditions = guards
                        .Select(g => new GuardCondition(g.Item1, InvocationInfo.Create(g.Item1, g.Item2)))
                        .ToList();
        }

        internal TransitionGuard(Func<bool> guard, string? description = null)
        {
            Conditions = new List<GuardCondition>
            {
                new(guard, InvocationInfo.Create(guard, description))
            };
        }

        internal TransitionGuard(IEnumerable<Tuple<Func<object[], bool>, string>> guards)
        {
            Conditions = guards
                        .Select(g => new GuardCondition(g.Item1, InvocationInfo.Create(g.Item1, g.Item2)))
                        .ToList();
        }

        internal TransitionGuard(Func<object[], bool> guard, string? description = null)
        {
            Conditions = new List<GuardCondition>
            {
                new(guard, InvocationInfo.Create(guard, description))
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