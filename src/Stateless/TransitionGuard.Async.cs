#if TASKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class TransitionGuard
        {
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

            internal TransitionGuard(Func<object[], Task<bool>> guard, string description = null)
            {
                Conditions = new List<GuardCondition>
                {
                    new GuardCondition(guard, Reflection.InvocationInfo.Create(guard, description))
                };
            }

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public async Task<ICollection<string>> UnmetGuardConditionsAsync(object[] args)
            {
                var guardTasks = Conditions
                    .Select(async c => new { GuardResult = await c.GuardAsync(args), c.Description });
                var conditions = await Task.WhenAll(guardTasks);

                return conditions
                    .Where(condition => !condition.GuardResult)
                    .Select(condition => condition.Description)
                    .ToList();
            }
        }
    }
}

#endif
