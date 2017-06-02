using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;
using Stateless.Reflection;

namespace TelephoneCallExample
{
    class Program
    {
        enum Trigger
        {
            CallDialed,
            CallConnected,
            LeftMessage,
            PlacedOnHold,
            TakenOffHold,
            PhoneHurledAgainstWall,
            MuteMicrophone,
            UnmuteMicrophone,
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

        static void Main(string[] args)
        {
            var phoneCall = new StateMachine<State, Trigger>(State.OffHook);
            setVolumeTrigger = phoneCall.SetTriggerParameters<int>(Trigger.SetVolume);

            phoneCall.Configure(State.OffHook)
	            .Permit(Trigger.CallDialed, State.Ringing);

            phoneCall.Configure(State.Ringing)
	            .Permit(Trigger.CallConnected, State.Connected);

            phoneCall.Configure(State.Connected)
                .OnEntry(t => StartCallTimer())
                .OnExit(t => StopCallTimer())
                .InternalTransition(Trigger.MuteMicrophone, t => OnMute())
                .InternalTransition(Trigger.UnmuteMicrophone, t => OnUnmute())
                .InternalTransition<int>(setVolumeTrigger, (volume, t) => OnSetVolume(volume))
                .Permit(Trigger.LeftMessage, State.OffHook)
	            .Permit(Trigger.PlacedOnHold, State.OnHold);

            phoneCall.Configure(State.OnHold)
                .SubstateOf(State.Connected)
                .Permit(Trigger.TakenOffHold, State.Connected)
                .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

            Print(phoneCall);
            Fire(phoneCall, Trigger.CallDialed);
            Print(phoneCall);
            Fire(phoneCall, Trigger.CallConnected);
            Print(phoneCall);
            SetVolume(phoneCall, 2);
            Print(phoneCall);
            Fire(phoneCall, Trigger.PlacedOnHold);
            Print(phoneCall);
            Fire(phoneCall, Trigger.MuteMicrophone);
            Print(phoneCall);
            Fire(phoneCall, Trigger.UnmuteMicrophone);
            Print(phoneCall);
            Fire(phoneCall, Trigger.TakenOffHold);
            Print(phoneCall);
            SetVolume(phoneCall, 11);
            Print(phoneCall);

            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
        }

        private static void OnSetVolume(int volume)
        {
            Console.WriteLine("Volume set to " + volume + "!");
        }

        private static void OnUnmute()
        {
            Console.WriteLine("Microphone unmuted!");
        }

        private static void OnMute()
        {
            Console.WriteLine("Microphone muted!");
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
        static void SetVolume(StateMachine<State, Trigger> phoneCall, int volume)
        {
            phoneCall.Fire(setVolumeTrigger, volume);
        }

        static void Print(StateMachine<State, Trigger> phoneCall)
        {
            Console.WriteLine("[Status:] {0}", phoneCall);
        }
    }
}
