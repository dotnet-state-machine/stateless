using System.Collections.Generic;
using System.Diagnostics;
using Stateless;

namespace BugTrackerExample
{
    public class AgencyApprovalIOWorkflow
    {
        private enum State
        {
            Draft,
            ReadyForAgencyApproval,
            AgencyApproved,
        }

        private State _currentState;
        private StateMachine<State, IOWorkflowTriggers> _stateMachine;
        StateMachine<State, IOWorkflowTriggers>.TriggerWithParameters<string> _assignAction;

        public AgencyApprovalIOWorkflow()
        {
            _stateMachine = new StateMachine<State, IOWorkflowTriggers>(() => _currentState, newState => _currentState = newState);
            _currentState = State.Draft; //hydrate initial state from DB

            _stateMachine.Configure(State.Draft)
                .OnEntry(OnEnter_Draft)
                .Permit(IOWorkflowTriggers.MakeReadyForAgencyApproval, State.ReadyForAgencyApproval);

            _stateMachine.Configure(State.ReadyForAgencyApproval)
                .OnEntry(OnEnter_AgencyApproved)
                .Permit(IOWorkflowTriggers.AgencyApprove, State.AgencyApproved)
                .Permit(IOWorkflowTriggers.AgencyReject, State.Draft);

            _stateMachine.Configure(State.AgencyApproved)
                .OnEntry(OnEnter_AgencyApproved);
        }

        public void MakeReadyForApproval()
        {
            _stateMachine.Fire(IOWorkflowTriggers.MakeReadyForAgencyApproval);
        }

        public IEnumerable<IOWorkflowTriggers> PermittedTriggers()
        {
            return _stateMachine.PermittedTriggers;
        }

        public void Approve()
        {
            _stateMachine.Fire(IOWorkflowTriggers.AgencyApprove);
        }

        public void Reject()
        {
            _stateMachine.Fire(IOWorkflowTriggers.AgencyReject);
        }

        private void OnEnter_Draft()
        {
            Debug.WriteLine($"writing {_currentState} to DB");
        }

        private void OnEnter_ReadyForAgencyApproval()
        {
            Debug.WriteLine($"writing {_currentState} to DB");
        }

        private void OnEnter_AgencyApproved()
        {
            Debug.WriteLine($"writing {_currentState} to DB");
        }
    }
}