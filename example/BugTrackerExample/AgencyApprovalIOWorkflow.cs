using System;
using System.Collections.Generic;
using System.Diagnostics;
using Stateless;
using Stateless.Graph;

namespace BugTrackerExample
{
    public class AgencyApprovalIOWorkflow
    {
        private enum State
        {
            Draft,
            ApprovalRejected,
            ReadyForAgencyApproval,
            AgencyApproved,
        }

        public class MakeReadyForAgencyApprovalParameters
        {
        }

        private State _currentState;
        private StateMachine<State, IOWorkflowTriggers> _stateMachine;
        StateMachine<State, IOWorkflowTriggers>.TriggerWithParameters<string> _assignAction;
        private StateMachine<State, IOWorkflowTriggers>.TriggerWithParameters<MakeReadyForAgencyApprovalParameters> _makeReadyForApproval;

        public AgencyApprovalIOWorkflow()
        {
            _stateMachine = new StateMachine<State, IOWorkflowTriggers>(() => _currentState, newState => _currentState = newState);
            _currentState = State.Draft; //hydrate initial state from DB

            _makeReadyForApproval = _stateMachine.SetTriggerParameters<MakeReadyForAgencyApprovalParameters>(IOWorkflowTriggers.MakeReadyForAgencyApproval);

            _stateMachine.Configure(State.Draft)
                .Permit(IOWorkflowTriggers.MakeReadyForAgencyApproval, State.ReadyForAgencyApproval);

            _stateMachine.Configure(State.ApprovalRejected)
                .SubstateOf(State.Draft);

            _stateMachine.Configure(State.ReadyForAgencyApproval)
                .OnEntryFrom(_makeReadyForApproval, parameters => OnEnter_ReadyForAgencyApproval(parameters))
                .Permit(IOWorkflowTriggers.AgencyApprove, State.AgencyApproved)
                .Permit(IOWorkflowTriggers.AgencyReject, State.ApprovalRejected);

            _stateMachine.Configure(State.AgencyApproved)
                .OnEntry(OnEnter_AgencyApproved);
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_stateMachine.GetInfo());
        }

        public void MakeReadyForApproval(MakeReadyForAgencyApprovalParameters parameters)
        {
            _stateMachine.Fire(_makeReadyForApproval, parameters);
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

        private void OnEnter_ReadyForAgencyApproval(MakeReadyForAgencyApprovalParameters parameters)
        {
            Debug.WriteLine($"writing {_currentState} to DB");
        }

        private void OnEnter_AgencyApproved()
        {
            Debug.WriteLine($"writing {_currentState} to DB");

            //throw new Exception("testing");
        }
    }
}