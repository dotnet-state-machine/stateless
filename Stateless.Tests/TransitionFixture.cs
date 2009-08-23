using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class TransitionFixture
    {
        [Test]
        public void IdentityTransitionIsNotChange()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 1, 0);
            Assert.IsTrue(t.IsReentry);
        }

        [Test]
        public void TransitioningTransitionIsChange()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 2, 0);
            Assert.IsFalse(t.IsReentry);
        }
    }
}
