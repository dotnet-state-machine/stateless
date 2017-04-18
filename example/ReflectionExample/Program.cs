using System;
using System.Linq;

using Stateless;
using Stateless.DotGraph;
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
                    foreach (MethodInfo methodInfo in trans.GuardConditionsMethodDescriptions)
                    {
                        Console.WriteLine("   Guard condition method \"" + methodInfo.Description + "\"");
                    }
                }
            }

            sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            Console.WriteLine("Exported as DOT graph:");
            Console.WriteLine(DotGraphFormatter.Format(sm.GetInfo()));
        }
    }
}
