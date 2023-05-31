using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests;

public class GetInfoFixture
{
    [Fact]
    public void GetInfo_should_return_Entry_action_with_trigger_name()
    {
        // ARRANGE
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.B)
            .OnEntryFrom(Trigger.X, () => { });
        
        // ACT
        var stateMachineInfo = sm.GetInfo();
        
        // ASSERT
        var stateInfo = Assert.Single(stateMachineInfo.States);
        var entryActionInfo = Assert.Single(stateInfo.EntryActions);
        Assert.Equal(Trigger.X.ToString(), entryActionInfo.FromTrigger);
    }
    
    [Fact]
    public void GetInfo_should_return_async_Entry_action_with_trigger_name()
    {
        // ARRANGE
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.B)
            .OnEntryFromAsync(Trigger.X, () => Task.CompletedTask);
        
        // ACT
        var stateMachineInfo = sm.GetInfo();
        
        // ASSERT
        var stateInfo = Assert.Single(stateMachineInfo.States);
        var entryActionInfo = Assert.Single(stateInfo.EntryActions);
        Assert.Equal(Trigger.X.ToString(), entryActionInfo.FromTrigger);
    }
}