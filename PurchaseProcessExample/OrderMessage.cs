using System;

namespace PurchaseProcessExample
{
    internal class OrderMessage
    {
        private readonly string _orderId;
        private readonly int _quantity;
        private readonly decimal _total;

        public decimal Total
        {
            get { return _total; }
        }

        public int Quantity
        {
            get { return _quantity; }
        }

        public string OrderId
        {
            get { return _orderId; }
        }

        public OrderMessage()
        {
            _orderId = Guid.NewGuid().ToString("N");
        }

        public OrderMessage(MutableOrder mutableOrder)
        {
            _orderId = mutableOrder.OrderId;
            _quantity = mutableOrder.Quantity;
            _total = mutableOrder.Total;
        }

        public MutableOrder ToMutable()
        {
            return new MutableOrder(this);
        }
    }
}