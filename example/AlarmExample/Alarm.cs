using AlarmExample;
using Stateless;
using System.Diagnostics;

namespace AlarmExample
{
    /// <summary>
    /// A sample class that implements an alarm as a state machine using Stateless 
    /// (https://github.com/dotnet-state-machine/stateless).
    /// 
    /// It also shows one way that temporary states can be implemented with the use of 
    /// Timers. PreArmed, PreTriggered, Triggered, and ArmPaused are "temporary" states with
    /// a configurable delay (i.e. to allow for an "arm delay"... a delay between Disarmed
    /// and Armed). The Triggered state is also considered temporary, since if an alarm 
    /// sounds for a certain period of time and no-one Acknowledges it, the state machine
    /// returns to the Armed state.
    /// 
    /// Timers are triggered via OnEntry() and OnExit() methods. Transitions are written to
    /// the Trace in order to show what happens.
    /// 
    /// The included PNG file shows what the state flow looks like.
    /// 
    /// </summary>
    public partial class Alarm
    {
        /// <summary>
        /// Moves the Alarm into the provided <see cref="AlarmState" /> via the defined <see cref="AlarmCommand" />.
        /// </summary>
        /// <param name="command">The <see cref="AlarmCommand" /> to execute on the current <see cref="AlarmState" />.</param>
        /// <returns>The new <see cref="AlarmState" />.</returns>
        public AlarmState ExecuteTransition(AlarmCommand command)
        {
            if (_machine.CanFire(command))
            {
                _machine.Fire(command);
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from {CurrentState} via {command}");
            }

            return CurrentState();
        }

        /// <summary>
        /// The current <see cref="AlarmState" /> of the alarm.
        /// </summary>
        public AlarmState CurrentState()
        {
            if (_machine != null)
                return _machine.State;
            else
                throw new InvalidOperationException("Alarm hasn't been configured yet.");
        }

        /// <summary>
        /// Defines whether the <see cref="Alarm"/> has been configured.
        /// </summary>
        public bool IsConfigured { get; private set; }

        /// <summary>
        /// Returns whether the provided command is a valid transition from the Current State.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool CanFireCommand(AlarmCommand command) 
        { 
            return _machine.CanFire(command); 
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="armDelay">The time (in seconds) the alarm will spend in the
        /// Prearmed status before continuing to the Armed status (if not transitioned to
        /// Disarmed via Disarm).</param>
        /// <param name="pauseDelay">The time (in seconds) the alarm will spend in the 
        /// ArmPaused status before returning to Armed (if not transitioned to Triggered 
        /// via Trigger).</param>
        /// <param name="triggerDelay">The time (in seconds) the alarm will spend in the
        /// PreTriggered status before continuing to the Triggered status (if not 
        /// transitioned to Disarmed via Disarm).</param>
        /// <param name="triggerTimeOut">The time (in seconds) the alarm will spend in the
        /// Triggered status before returning to the Armed status (if not transitioned to
        /// Disarmed via Disarm).</param>
        public Alarm(int armDelay, int pauseDelay, int triggerDelay, int triggerTimeOut)
        {
            _machine = new StateMachine<AlarmState, AlarmCommand>(AlarmState.Undefined);

            preArmTimer = new System.Timers .Timer(armDelay * 1000) { AutoReset = false, Enabled = false };
            preArmTimer.Elapsed += TimeoutTimerElapsed;
            pauseTimer = new System.Timers.Timer(pauseDelay * 1000) { AutoReset = false, Enabled = false };
            pauseTimer.Elapsed += TimeoutTimerElapsed;
            triggerDelayTimer = new System.Timers.Timer(triggerDelay * 1000) { AutoReset = false, Enabled = false };
            triggerDelayTimer.Elapsed += TimeoutTimerElapsed;
            triggerTimeOutTimer = new System.Timers.Timer(triggerTimeOut * 1000) { AutoReset = false, Enabled = false };
            triggerTimeOutTimer.Elapsed += TimeoutTimerElapsed;

            _machine.OnTransitioned(OnTransition);

            _machine.Configure(AlarmState.Undefined)
                .Permit(AlarmCommand.Startup, AlarmState.Disarmed)
                .OnExit(() => IsConfigured = true);

            _machine.Configure(AlarmState.Disarmed)
                .Permit(AlarmCommand.Arm, AlarmState.Prearmed);

            _machine.Configure(AlarmState.Armed)
                .Permit(AlarmCommand.Disarm, AlarmState.Disarmed)
                .Permit(AlarmCommand.Trigger, AlarmState.PreTriggered)
                .Permit(AlarmCommand.Pause, AlarmState.ArmPaused);

            _machine.Configure(AlarmState.Prearmed)
                .OnEntry(() => ConfigureTimer(true, preArmTimer, "Pre-arm"))
                .OnExit(() => ConfigureTimer(false, preArmTimer, "Pre-arm"))
                .Permit(AlarmCommand.TimeOut, AlarmState.Armed)
                .Permit(AlarmCommand.Disarm, AlarmState.Disarmed);

            _machine.Configure(AlarmState.ArmPaused)
                .OnEntry(() => ConfigureTimer(true, pauseTimer, "Pause delay"))
                .OnExit(() => ConfigureTimer(false, pauseTimer, "Pause delay"))
                .Permit(AlarmCommand.TimeOut, AlarmState.Armed)
                .Permit(AlarmCommand.Trigger, AlarmState.PreTriggered);

            _machine.Configure(AlarmState.Triggered)
                .OnEntry(() => ConfigureTimer(true, triggerTimeOutTimer, "Trigger timeout"))
                .OnExit(() => ConfigureTimer(false, triggerTimeOutTimer, "Trigger timeout"))
                .Permit(AlarmCommand.TimeOut, AlarmState.Armed)
                .Permit(AlarmCommand.Acknowledge, AlarmState.Acknowledged);

            _machine.Configure(AlarmState.PreTriggered)
                .OnEntry(() => ConfigureTimer(true, triggerDelayTimer, "Trigger delay"))
                .OnExit(() => ConfigureTimer(false, triggerDelayTimer, "Trigger delay"))
                .Permit(AlarmCommand.TimeOut, AlarmState.Triggered)
                .Permit(AlarmCommand.Disarm, AlarmState.Disarmed);

            _machine.Configure(AlarmState.Acknowledged)
                .Permit(AlarmCommand.Disarm, AlarmState.Disarmed);

            _machine.Fire(AlarmCommand.Startup);
        }

        private void TimeoutTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _machine.Fire(AlarmCommand.TimeOut);
        }

        private void ConfigureTimer(bool active, System.Timers.Timer timer, string timerName)
        {
            if (timer != null)
                if (active)
                {
                    timer.Start();
                    Trace.WriteLine($"{timerName} started.");
                }
                else
                {
                    timer.Stop();
                    Trace.WriteLine($"{timerName} cancelled.");
                }
        }

        private void OnTransition(StateMachine<AlarmState, AlarmCommand>.Transition transition)
        {
            Trace.WriteLine($"Transitioned from {transition.Source} to " +
                $"{transition.Destination} via {transition.Trigger}.");
        }
        
        private StateMachine<AlarmState, AlarmCommand> _machine;
        private System.Timers.Timer? preArmTimer;
        private System.Timers.Timer? pauseTimer;
        private System.Timers.Timer? triggerDelayTimer;
        private System.Timers.Timer? triggerTimeOutTimer;
    }
}