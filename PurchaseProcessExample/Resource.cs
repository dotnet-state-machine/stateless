namespace PurchaseProcessExample
{
    abstract class Resource
    {
        protected IBus _bus;
        public abstract OrderResourceType ResourceType { get; }

        public void Subscribe(IBus bus)
        {
            bus.Register(this);
            _bus = bus;
        }

        public abstract void ReceiveMessage(OrderMessage message);
    }
}
