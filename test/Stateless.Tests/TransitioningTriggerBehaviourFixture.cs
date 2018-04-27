﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Stateless.Tests
{
    public class TransitioningTriggerBehaviourFixture
    {
        [Fact]
        public void TransitionsToDestinationState()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, null, true);
            Assert.True(transtioning.ResultsInTransitionFrom(State.B, new object[0], out State destination));
            Assert.Equal(State.C, destination);
        }
    }
}
