using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;

namespace TelephoneCallExample
{
    class Program
    {
        enum Trigger
        {
            CallDialed,
            HungUp,
            CallConnected,
            LeftMessage,
            PlacedOnHold,
            TakenOffHold,
            PhoneHurledAgainstWall,
            MuteMicrophone,
            SetVolume
        }

        enum State
        {
            OffHook,
            Ringing,
            Connected,
            OnHold,
            PhoneDestroyed
        }
        static StateMachine<State, Trigger>.TriggerWithParameters<int> setVolumeTrigger;
        static StateMachine<State, Trigger>.TriggerWithParameters<int> muteMicTrigger;

        static void Main(string[] args)
        {
            var phoneCall = new StateMachine<State, Trigger>(State.OffHook);

            setVolumeTrigger = phoneCall.SetTriggerParameters<int>(Trigger.SetVolume);
            muteMicTrigger = phoneCall.SetTriggerParameters<int>(Trigger.MuteMicrophone);

            phoneCall.Configure(State.OffHook)
	            .Permit(Trigger.CallDialed, State.Ringing);
            	
            phoneCall.Configure(State.Ringing)
	            .Permit(Trigger.HungUp, State.OffHook)
	            .Permit(Trigger.CallConnected, State.Connected);

            phoneCall.Configure(State.Connected)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())
                .Permit(Trigger.LeftMessage, State.OffHook)
                .Permit(Trigger.HungUp, State.OffHook)
                .Permit(Trigger.PlacedOnHold, State.OnHold)
                .InternalTransistion(muteMicTrigger, (volume, t) => OnMuteMicrophone())
                .InternalTransistion(setVolumeTrigger, (volume, t) => OnSetVolume(volume));

            phoneCall.Configure(State.OnHold)
                .SubstateOf(State.Connected)
                .Permit(Trigger.TakenOffHold, State.Connected)
                .Permit(Trigger.HungUp, State.OffHook)
                .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

            Print(phoneCall);
            Fire(phoneCall, Trigger.CallDialed);
            Print(phoneCall);
            Fire(phoneCall, Trigger.CallConnected);
            Print(phoneCall);
            Fire(phoneCall, Trigger.PlacedOnHold);
            Print(phoneCall);
            Fire(phoneCall, setVolumeTrigger, 11);
            Print(phoneCall);
            Fire(phoneCall, muteMicTrigger, 0);
            Print(phoneCall);
            Fire(phoneCall, Trigger.TakenOffHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.HungUp);
            Print(phoneCall);

            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
        }



        static void StartCallTimer()
        {
            Console.WriteLine("[Timer:] Call started at {0}", DateTime.Now);
        }

        static void StopCallTimer()
        {
            Console.WriteLine("[Timer:] Call ended at {0}", DateTime.Now);
        }

        static void Fire(StateMachine<State, Trigger> phoneCall, Trigger trigger)
        {
            Console.WriteLine("[Firing:] {0}", trigger);
            phoneCall.Fire(trigger);
        }
        private static void Fire(StateMachine<State, Trigger> phoneCall, StateMachine<State, Trigger>.TriggerWithParameters<int> trigger, int payload)
        {
            Console.WriteLine("[Firing:] {0}", trigger.Trigger);
            phoneCall.Fire(trigger, payload);
        }
        static void Print(StateMachine<State, Trigger> phoneCall)
        {
            Console.WriteLine("[Status:] {0}", phoneCall);
        }
        private static void OnMuteMicrophone()
        {
            Console.WriteLine("Microphone muted!");
        }
        private static void OnSetVolume(int volume)
        {
            int vol = (int)volume;
            Console.WriteLine("Volume set to " + vol + "!");
        }
    }
}
