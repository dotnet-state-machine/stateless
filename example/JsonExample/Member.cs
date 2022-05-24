using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stateless;

namespace JsonExample;

public class Member {
    public enum MembershipState {
        Inactive,
        Active,
        Terminated
    }

    private readonly StateMachine<MembershipState, MemberTriggers> _stateMachine;
    public           MembershipState                               State => _stateMachine.State;
    public           string                                        Name  { get; }

    public Member(string name) {
        _stateMachine = new StateMachine<MembershipState, MemberTriggers>(MembershipState.Active);
        Name          = name;

        ConfigureStateMachine();
    }

    [JsonConstructor]
    private Member(string state, string name) {
        var memberState = (MembershipState) Enum.Parse(typeof(MembershipState), state);
        _stateMachine = new StateMachine<MembershipState, MemberTriggers>(memberState);
        Name          = name;

        ConfigureStateMachine();
    }

    private void ConfigureStateMachine() {
        _stateMachine.Configure(MembershipState.Active)
                     .Permit(MemberTriggers.Suspend, MembershipState.Inactive)
                     .Permit(MemberTriggers.Terminate, MembershipState.Terminated);

        _stateMachine.Configure(MembershipState.Inactive)
                     .Permit(MemberTriggers.Reactivate, MembershipState.Active)
                     .Permit(MemberTriggers.Terminate, MembershipState.Terminated);

        _stateMachine.Configure(MembershipState.Terminated)
                     .Permit(MemberTriggers.Reactivate, MembershipState.Active);
    }

    public async Task TerminateAsync() {
        await _stateMachine.FireAsync(MemberTriggers.Terminate);
    }

    public async Task SuspendAsync() {
        await _stateMachine.FireAsync(MemberTriggers.Suspend);
    }

    public async Task ReactivateAsync() {
        await _stateMachine.FireAsync(MemberTriggers.Reactivate);
    }

    public string ToJson() => JsonConvert.SerializeObject(this);

    public static Member FromJson(string jsonString) => JsonConvert.DeserializeObject<Member>(jsonString);

    public bool Equals(Member anotherMember) => State == anotherMember.State && Name == anotherMember.Name;

    private enum MemberTriggers {
        Suspend,
        Terminate,
        Reactivate
    }
}