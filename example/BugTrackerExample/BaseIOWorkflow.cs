using System;
using System.Collections.Generic;
using System.Diagnostics;
using Stateless;

namespace BugTrackerExample
{
    public enum IOWorkflowTriggers
    {
        Create,
        Save,
        AgencySignoff,
        PublisherSignoff,
        MakeReadyForAgencyApproval,
        AgencyApprove,
        AgencyReject,
        AgencyRecall,
        PublisherRecall,
    }

    public class BaseIOWorkflow
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

        private Action<State> _persistStateAction = null;
        private Func<State> _hydrateStateFunc = null;

        public BaseIOWorkflow()
        {
            _stateMachine = new StateMachine<State, IOWorkflowTriggers>(() => _currentState, newState => _currentState = newState);
            _currentState = State.Draft; //hydrate initial state from DB
            if (_hydrateStateFunc != null)
                _currentState = _hydrateStateFunc();

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
            _persistStateAction?.Invoke(_currentState);
            Debug.WriteLine($"writing {_currentState} to DB");
        }

        private void OnEnter_ReadyForAgencyApproval()
        {
            _persistStateAction?.Invoke(_currentState);
            Debug.WriteLine($"writing {_currentState} to DB");
        }

        private void OnEnter_AgencyApproved()
        {
            _persistStateAction?.Invoke(_currentState);
            Debug.WriteLine($"writing {_currentState} to DB");
        }
    }
}