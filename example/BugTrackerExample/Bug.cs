using System;
using Stateless;
using Stateless.Graph;

namespace BugTrackerExample
{
    public class Bug
    {
        private enum State { Open, Assigned, Deferred, Closed }

        private enum Trigger { Assign, Defer, Close }

        private readonly StateMachine<State, Trigger> _machine;
        // The TriggerWithParameters object is used when a trigger requires a payload.
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string> _assignTrigger;

        private readonly string _title;
        private string _assignee;


        /// <summary>
        /// Constructor for the Bug class
        /// </summary>
        /// <param name="title">The title of the bug report</param>
        public Bug(string title)
        {
            _title = title;

            // Instantiate a new state machine in the Open state
            _machine = new StateMachine<State, Trigger>(State.Open);

            // Instantiate a new trigger with a parameter. 
            _assignTrigger = _machine.SetTriggerParameters<string>(Trigger.Assign);

            // Configure the Open state
            _machine.Configure(State.Open)
                .Permit(Trigger.Assign, State.Assigned);

            // Configure the Assigned state
            _machine.Configure(State.Assigned)
                .SubstateOf(State.Open)
                .OnEntryFrom(_assignTrigger, OnAssigned)  // This is where the TriggerWithParameters is used. Note that the TriggerWithParameters object is used, not something from the enum
                .PermitReentry(Trigger.Assign)
                .Permit(Trigger.Close, State.Closed)
                .Permit(Trigger.Defer, State.Deferred)
                .OnExit(OnDeassigned);

            // Configure the Deferred state
            _machine.Configure(State.Deferred)
                .OnEntry(() => _assignee = null)
                .Permit(Trigger.Assign, State.Assigned);
        }

        public void Close()
        {
            _machine.Fire(Trigger.Close);
        }

        public void Assign(string assignee)
        {
            // This is how a trigger with parameter is used, the parameter is supplied to the state machine as a parameter to the Fire method.
            _machine.Fire(_assignTrigger, assignee);
        }

        public bool CanAssign => _machine.CanFire(Trigger.Assign);

        public void Defer()
        {
            _machine.Fire(Trigger.Defer);
        }
        /// <summary>
        /// This method is called automatically when the Assigned state is entered, but only when the trigger is _assignTrigger.
        /// </summary>
        /// <param name="assignee"></param>
        private void OnAssigned(string assignee)
        {
            if (_assignee != null && assignee != _assignee)
                SendEmailToAssignee("Don't forget to help the new employee!");

            _assignee = assignee;
            SendEmailToAssignee("You own it.");
        }
        /// <summary>
        /// This method is called when the state machine exits the Assigned state
        /// </summary>
        private void OnDeassigned()
        {
            SendEmailToAssignee("You're off the hook.");
        }

        private void SendEmailToAssignee(string message)
        {
            Console.WriteLine("{0}, RE {1}: {2}", _assignee, _title, message);
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }
    }
}
