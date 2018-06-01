using Xunit;

namespace Stateless.Tests
{
    public class TransitionFixture
    {
        [Fact]
        public void IdentityTransitionIsNotChange()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 1, 0);
            Assert.True(t.IsReentry);
        }

        [Fact]
        public void TransitioningTransitionIsChange()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 2, 0);
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
                    t => { return true; },
                    () =>
                    {
                        Assert.True(true);
                    })
                .InternalTransitionIf(
                    1,
                    u => { return false; },
                    () =>
                    {
                        Assert.True(false);
                    });

            machine.Fire(1);
        }
    }
}
