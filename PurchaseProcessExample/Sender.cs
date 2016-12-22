using System;

namespace PurchaseProcessExample
{
    class Sender : Resource
    {
        public override OrderResourceType ResourceType
        {
            get { return OrderResourceType.Sender; }
        }

        public override void ReceiveMessage(OrderMessage message)
        {
            Console.WriteLine("You have bougth a quantity of " + message.Quantity + " and paid " + message.Total + " $");
            Console.WriteLine("The products will be shipped soon");
            Console.WriteLine("Thank you for your purchase!");
            while (true)
            {
                Console.WriteLine(@"Digit O to make a new order or E to Exit");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == 'E' || key.KeyChar == 'e')
                {
                    _bus.SendMessage(message, OrderEvent.Exit);
                    Console.WriteLine("Bye!");
                    Console.ReadKey();
                    break;
                }
                if (key.KeyChar == 'O' || key.KeyChar == 'o')
                {
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine();
                    _bus.SendMessage(new OrderMessage(), OrderEvent.Access);
                    break;
                }
                else Console.WriteLine(@"Input not valid");
            }
        }
    }
}
