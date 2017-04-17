using System;
using System.Linq;

using Stateless;
using Stateless.Reflection;

namespace ReflectionExample
{
    class Program
    {
        enum State
        {
            A, B, C, D
        }

        enum Trigger
        {
            X, Y, Z
        }

        static void Main(string[] args)
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            StateMachineInfo inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                foreach (FixedTransitionInfo trans in stateInfo.FixedTransitions)
                {
                    Console.WriteLine("State " + stateInfo.UnderlyingState + " transitions to " + trans.DestinationState
                        + ", has " + trans.GuardConditionsMethodDescriptions.Count() + " guard conditions");
                    // Assert.Equal(0, trans.GuardConditionsMethodDescriptions.Count());
                    foreach (MethodInfo methodInfo in trans.GuardConditionsMethodDescriptions)
                    {
                        Console.WriteLine("   Guard condition method \"" + methodInfo.Description + "\"");
                    }
                }
            }
        }
    }
}
