using System;
using System.Threading.Tasks;
using Stateless;
using Stateless.Graph;

namespace TelephoneCallExample; 

public class PhoneCall
{
    private enum Trigger
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

    private enum State
    {
        OffHook,
        Ringing,
        Connected,
        OnHold,
        PhoneDestroyed
    }

    private State _state = State.OffHook;

    private readonly StateMachine<State, Trigger>                  _machine;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<int> _setVolumeTrigger;

    private readonly StateMachine<State, Trigger>.TriggerWithParameters<string> _setCalleeTrigger;

    private readonly string _caller;

    private string _callee;

    public PhoneCall(string caller)
    {
        _caller  = caller;
        _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

        _setVolumeTrigger = _machine.SetTriggerParameters<int>(Trigger.SetVolume);
        _setCalleeTrigger = _machine.SetTriggerParameters<string>(Trigger.CallDialed);

        _machine.Configure(State.OffHook)
                .Permit(Trigger.CallDialed, State.Ringing);

        _machine.Configure(State.Ringing)
                .OnEntryFrom(_setCalleeTrigger, callee => OnDialed(callee), "Caller number to call")
                .Permit(Trigger.CallConnected, State.Connected);

        _machine.Configure(State.Connected)
                .OnEntry(_ => StartCallTimer())
                .OnExit(_ => StopCallTimer())
                .InternalTransition(Trigger.MuteMicrophone, _ => OnMute())
                .InternalTransition(Trigger.UnmuteMicrophone, _ => OnUnmute())
                .InternalTransition(_setVolumeTrigger, (volume, _) => OnSetVolume(volume))
                .Permit(Trigger.LeftMessage, State.OffHook)
                .Permit(Trigger.PlacedOnHold, State.OnHold);

        _machine.Configure(State.OnHold)
                .SubstateOf(State.Connected)
                .Permit(Trigger.TakenOffHold, State.Connected)
                .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

        _machine.OnTransitioned(t => Console.WriteLine($"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ",  t.Parameters)})"));
    }

    private static void OnSetVolume(int volume)
    {
        Console.WriteLine($"Volume set to {volume}!");
    }

    private static void OnUnmute()
    {
        Console.WriteLine("Microphone unmuted!");
    }

    private static void OnMute()
    {
        Console.WriteLine("Microphone muted!");
    }

    private void OnDialed(string callee)
    {
        _callee = callee;
        Console.WriteLine("[Phone Call] placed for : [{0}]", _callee);
    }

    private static void StartCallTimer()
    {
        Console.WriteLine("[Timer:] Call started at {0}", DateTime.Now);
    }

    private static void StopCallTimer()
    {
        Console.WriteLine("[Timer:] Call ended at {0}", DateTime.Now);
    }

    public async Task MuteAsync()
    {
        await _machine.FireAsync(Trigger.MuteMicrophone);
    }

    public async Task UnmuteAsync()
    {
        await _machine.FireAsync(Trigger.UnmuteMicrophone);
    }

    public async Task SetVolumeAsync(int volume)
    {
        await _machine.FireAsync(_setVolumeTrigger, volume);
    }

    public void Print()
    {
        Console.WriteLine("[{1}] placed call and [Status:] {0}", _machine.State, _caller);
    }

    public async Task DialedAsync(string callee)
    {           
        await _machine.FireAsync(_setCalleeTrigger, callee);
    }

    public async Task ConnectedAsync()
    {
        await _machine.FireAsync(Trigger.CallConnected);
    }

    public async Task HoldAsync()
    {
        await _machine.FireAsync(Trigger.PlacedOnHold);
    }

    public async Task ResumeAsync()
    {
        await _machine.FireAsync(Trigger.TakenOffHold);
    }

    public string ToDotGraph()
    {
        return UmlDotGraph.Format(_machine.GetInfo());
    }
}