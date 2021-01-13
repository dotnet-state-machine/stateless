using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class InternalTransitionAsyncFixture
    {
        [Fact]
        public async Task Stateless_InternalTransitionAsyncIf_Guard()
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
