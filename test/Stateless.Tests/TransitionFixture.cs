using Xunit;

namespace Stateless.Tests; 

public class TransitionFixture
{
    [Fact]
    public void IdentityTransitionIsNotChange()
    {
        var t = new StateMachine<int, int>.Transition(1, 1, 0);
        Assert.True(t.IsReentry);
    }

    [Fact]
    public void TransitioningTransitionIsChange()
    {
        var t = new StateMachine<int, int>.Transition(1, 2, 0);
        Assert.False(t.IsReentry);
    }

    [Fact]
    public void TestInternalIf()
    {
        // Verifies that only one internal action is executed
        var machine = new StateMachine<int, int>(1);

        machine.Configure(1)
               .InternalTransitionIf(
                                     1,
                                     _ => { return true; },
                                     () =>
                                     {
                                         Assert.True(true);
                                     })
               .InternalTransitionIf(
                                     1,
                                     _ => { return false; },
                                     () =>
                                     {
                                         Assert.True(false);
                                     });

        machine.Fire(1);
    }
}