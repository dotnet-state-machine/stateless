using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateless.Reflection;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitionGuardAsync
        {
            internal IList<GuardConditionAsync> Conditions { get; }

            public static readonly TransitionGuardAsync Empty = new TransitionGuardAsync(new Tuple<Func<object[],Task<bool>>, string>[0]);

            #region Generic TArg0, ... to object[] converters

            public static Func<object[], Task<bool>> ToPackedGuard<TArg0>(Func<TArg0, Task<bool>> guard)
            {
                return args => guard(ParameterConversion.Unpack<TArg0>(args, 0));
            }

            public static Func<object[], Task<bool>> ToPackedGuard<TArg0, TArg1>(Func<TArg0, TArg1, Task<bool>> guard)
            {
                return args => guard(
                    ParameterConversion.Unpack<TArg0>(args, 0), 
                    ParameterConversion.Unpack<TArg1>(args, 1));
            }

            public static Func<object[], Task<bool>> ToPackedGuard<TArg0, TArg1, TArg2>(Func<TArg0, TArg1, TArg2, Task<bool>> guard)
            {
                return args => guard(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2));
            }

            public static Tuple<Func<object[], Task<bool>>, string>[] ToPackedGuards<TArg0>(Tuple<Func<TArg0, Task<bool>>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], Task<bool>>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            public static Tuple<Func<object[], Task<bool>>, string>[] ToPackedGuards<TArg0, TArg1>(Tuple<Func<TArg0, TArg1, Task<bool>>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], Task<bool>>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            public static Tuple<Func<object[], Task<bool>>, string>[] ToPackedGuards<TArg0, TArg1, TArg2>(Tuple<Func<TArg0, TArg1, TArg2, Task<bool>>, string>[] guards)
            {
                return guards.Select(guard => new Tuple<Func<object[], Task<bool>>, string>(
                        ToPackedGuard(guard.Item1), guard.Item2))
                    .ToArray();
            }

            #endregion

            internal TransitionGuardAsync(Tuple<Func<Task<bool>>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardConditionAsync(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2, InvocationInfo.Timing.Asynchronous)))
                    .ToList();
            }

            internal TransitionGuardAsync(Func<Task<bool>> guard, string description = null)
            {
                Conditions = new List<GuardConditionAsync>
                {
                    new GuardConditionAsync(guard, Reflection.InvocationInfo.Create(guard, description, InvocationInfo.Timing.Asynchronous))
                };
            }

            internal TransitionGuardAsync(Tuple<Func<object[], Task<bool>>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardConditionAsync(g.Item1, Reflection.InvocationInfo.Create(g.Item1, g.Item2, InvocationInfo.Timing.Asynchronous)))
                    .ToList();
            }

            internal TransitionGuardAsync(Func<object[], Task<bool>> guard, string description = null)
            {
                Conditions = new List<GuardConditionAsync>
                {
                    new GuardConditionAsync(guard, Reflection.InvocationInfo.Create(guard, description, InvocationInfo.Timing.Asynchronous))
                };
            }
            
            /// <summary>
            /// Guards is the list of the guard functions for all guard conditions for this transition
            /// </summary>
            internal ICollection<Func<object[], Task<bool>>> Guards => Conditions.Select(g => g.GuardAsync).ToList();

            /// <summary>
            /// GuardConditionsMet is true if all of the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public async Task<bool> GuardConditionsMet(object[] args)
            {
                foreach (var condition in Conditions)
                {
                    if (condition == null) continue;
                    var isValid = await condition.GuardAsync(args);
                    if (!isValid) return false;
                }

                return true;
            }

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public async Task<ICollection<string>> UnmetGuardConditions(object[] args)
            {
                var descriptions = new List<string>();

                foreach (var condition in Conditions)
                {
                    if (!await condition.GuardAsync(args))
                    {
                        descriptions.Add(condition.Description);
                    }
                }

                return descriptions;
            }
        }
    }
}