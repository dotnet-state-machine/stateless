using System;

namespace PurchaseProcessExample
{
    class Seller : Resource
    {
        public override OrderResourceType ResourceType
        {
            get { return OrderResourceType.Seller; }
        }

        public override void ReceiveMessage(OrderMessage message)
        {
            Console.WriteLine("Total to pay: " + message.Total);
            while (true)
            {
                Console.WriteLine(@"Digit P to pay or M to Modify the product quantity");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == 'P' || key.KeyChar == 'p')
                {
                    _bus.SendMessage(message, OrderEvent.Pay);
                    break;
                }
                if (key.KeyChar == 'M' || key.KeyChar == 'm')
                {
                    _bus.SendMessage(message, OrderEvent.Modify);
                    break;
                }
                else Console.WriteLine(@"Input not valid");
            }
        }
    }
}
