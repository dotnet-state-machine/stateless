namespace PurchaseProcessExample
{
    internal class MutableOrder
    {
        public decimal Total { get; set; }

        public int Quantity { get; set; }

        public string OrderId { get; set; }

        public OrderMessage ToOrder()
        {
            return new OrderMessage(this);
        }

        public MutableOrder(OrderMessage order)
        {
            OrderId = order.OrderId;
            Quantity = order.Quantity;
            Total = order.Total;
        }
    }
}