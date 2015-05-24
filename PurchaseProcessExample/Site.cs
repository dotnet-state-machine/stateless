using System;

namespace PurchaseProcessExample
{
    class Site : Resource
    {
        public override OrderResourceType ResourceType { get { return OrderResourceType.Site; } }

        public override void ReceiveMessage(OrderMessage message)
        {
            throw new NotImplementedException();
        }

        public void EnterNewOrder()
        {
            _bus.SendMessage(new OrderMessage(), OrderEvent.Access);
        }
    }
}
