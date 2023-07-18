using System;
using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class InternalTransitionAsyncFixture
    {
        [Fact]
        public async Task InternalTransitionAsyncIf_AllowGuardWithParameter()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            const int intParam = 5;
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, i =>
                {
                    guardInvoked = true;
                    Assert.Equal(intParam, i);
                    return true;
                }, (i, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }
    
        [Fact]
        public async Task InternalTransitionAsyncIf_AllowGuardWithTwoParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            const int intParam = 5;
            const string stringParam = "5";
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, (i, s) =>
                {
                    guardInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    return true;
                }, (i, s, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam, stringParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }
    
        [Fact]
        public async Task InternalTransitionAsyncIf_AllowGuardWithThreeParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            const int intParam = 5;
            const string stringParam = "5";
            const bool boolParam = true;
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, (i, s, b) =>
                {
                    guardInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    Assert.Equal(boolParam, b);
                    return true;
                }, (i, s, b, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    Assert.Equal(boolParam, b);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam, stringParam, boolParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }

        [Fact]
        [Obsolete]
        public async Task InternalTransitionAsyncIf_DeprecatedOverload_AllowGuardWithoutParameter()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            const int intParam = 5;
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, () =>
                {
                    guardInvoked = true;
                    return true;
                }, (i, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }

        [Fact]
        [Obsolete]
        public async Task InternalTransitionAsyncIf_DeprecatedOverload_AllowGuardWithParameter()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            const int intParam = 5;
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, () =>
                {
                    guardInvoked = true;
                    return true;
                }, (i, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }

        [Fact]
        [Obsolete]
        public async Task InternalTransitionAsyncIf_DeprecatedOverload_AllowGuardWithTwoParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            const int intParam = 5;
            const string stringParam = "5";
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, () =>
                {
                    guardInvoked = true;
                    return true;
                }, (i, s, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam, stringParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }

        [Fact]
        [Obsolete]
        public async Task InternalTransitionAsyncIf_DeprecatedOverload_AllowGuardWithThreeParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            const int intParam = 5;
            const string stringParam = "5";
            const bool boolParam = true;
            var guardInvoked = false;
            var callbackInvoked = false;

            sm.Configure(State.A)
                .InternalTransitionAsyncIf(trigger, () =>
                {
                    guardInvoked = true;
                    return true;
                }, (i, s, b, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(stringParam, s);
                    Assert.Equal(boolParam, b);
                    return Task.CompletedTask;
                });

            await sm.FireAsync(trigger, intParam, stringParam, boolParam);

            Assert.True(guardInvoked);
            Assert.True(callbackInvoked);
        }

        [Fact]
        [Obsolete]
        public async Task InternalTransitionAsyncIf_DeprecatedOverload_GuardExecutedOnlyOnce()
        {
            var guardCalls = 0;
            var order = new Order
            {
                Status = OrderStatus.OrderPlaced,
                PaymentStatus = PaymentStatus.Pending,
            };
            var stateMachine = new StateMachine<OrderStatus, OrderStateTrigger>(order.Status);
            stateMachine.Configure(OrderStatus.OrderPlaced)
                 .InternalTransitionAsyncIf<int>(OrderStateTrigger.PaymentCompleted,
                       () => PreCondition(ref guardCalls),
                       _ => ChangePaymentState(order, PaymentStatus.Completed));

            await stateMachine.FireAsync(OrderStateTrigger.PaymentCompleted);

            Assert.Equal(1, guardCalls);
        }

        /// <summary>
        /// This unit test demonstrated bug report #417
        /// </summary>
        [Fact]
        public async Task InternalTransitionAsyncIf_GuardExecutedOnlyOnce()
        {
            var guardCalls = 0;
            var order = new Order
            {
                Status = OrderStatus.OrderPlaced,
                PaymentStatus = PaymentStatus.Pending,
            };
            var stateMachine = new StateMachine<OrderStatus, OrderStateTrigger>(order.Status);
            stateMachine.Configure(OrderStatus.OrderPlaced)
                 .InternalTransitionAsyncIf(OrderStateTrigger.PaymentCompleted,
                       () => PreCondition(ref guardCalls),
                       () => ChangePaymentState(order, PaymentStatus.Completed));

            await stateMachine.FireAsync(OrderStateTrigger.PaymentCompleted);

            Assert.Equal(1, guardCalls);
        }

        private bool PreCondition(ref int calls)
        {
            calls++;
            return true;
        }

        private async Task ChangePaymentState(Order order, PaymentStatus paymentStatus)
        {
            await Task.FromResult(order.PaymentStatus = paymentStatus);
        }

        private enum OrderStatus { OrderPlaced }
        private enum PaymentStatus { Pending, Completed }
        private enum OrderStateTrigger { PaymentCompleted }
        private class Order
        {
            public OrderStatus Status { get; internal set; }
            public PaymentStatus PaymentStatus { get; internal set; }
        }
    }
}