using System;
using Newtonsoft.Json;
using Stateless;

namespace JsonExample
{
    public class Member
    {
        private enum MemberTriggers
        {
            Suspend,
            Terminate,
            Reactivate
        }
        public enum MembershipState
        {
            Inactive,
            Active,
            Terminated
        }
        public MembershipState State => _stateMachine.State;
        public string Name { get; }
        private readonly StateMachine<MembershipState, MemberTriggers> _stateMachine;

        public Member(string name)
        {
            _stateMachine = new StateMachine<MembershipState, MemberTriggers>(MembershipState.Active);
            Name = name;

            ConfigureStateMachine();
        }

        [JsonConstructor]
        private Member(string state, string name)
        {
            var memberState = (MembershipState)Enum.Parse(typeof(MembershipState), state);
            _stateMachine = new StateMachine<MembershipState, MemberTriggers>(memberState);
            Name = name;

            ConfigureStateMachine();
        }

        private void ConfigureStateMachine()
        {
            _stateMachine.Configure(MembershipState.Active)
                .Transition(MemberTriggers.Suspend).To(MembershipState.Inactive)
                .Transition(MemberTriggers.Terminate).To(MembershipState.Terminated);

            _stateMachine.Configure(MembershipState.Inactive)
                .Transition(MemberTriggers.Reactivate).To(MembershipState.Active)
                .Transition(MemberTriggers.Terminate).To(MembershipState.Terminated);

            _stateMachine.Configure(MembershipState.Terminated)
                .Transition(MemberTriggers.Reactivate).To(MembershipState.Active);
        }

        public void Terminate()
        {
            _stateMachine.Fire(MemberTriggers.Terminate);
        }

        public void Suspend()
        {
            _stateMachine.Fire(MemberTriggers.Suspend);
        }

        public void Reactivate()
        {
            _stateMachine.Fire(MemberTriggers.Reactivate);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Member FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<Member>(jsonString);
        }

        public bool Equals(Member anotherMember)
        {
            return ((State == anotherMember.State) && (Name == anotherMember.Name));
        }
    }




}
