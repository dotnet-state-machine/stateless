using System;

namespace PurchaseProcessExample
{
    class Shop : Resource
    {
        public override OrderResourceType ResourceType { get {return OrderResourceType.Shop;}  }

        public override void ReceiveMessage(OrderMessage message)
        {
            Console.WriteLine("Welcome into the MonoProd shop!");
            Console.WriteLine("The unit price is 34 $");
            Console.WriteLine("You have " + message.Quantity + " products in the basket");
            MutableOrder provisory = message.ToMutable();
            while(true)
            {
                Console.WriteLine("Digit product quantity to Order or E to Exit and then press enter");
                string value = Console.ReadLine();
                if (value == null) continue;
                if (value.Trim().ToUpper() == "E")
                {
                    _bus.SendMessage(message, OrderEvent.Exit);
                    Console.WriteLine(@"You have exited without buying");
                    Console.ReadKey();
                    break;
                }
                int quantity = 0;
                if (int.TryParse(value, out quantity))
                {
                    provisory.Quantity = quantity;
                    provisory.Total = quantity*34;
                    _bus.SendMessage(provisory.ToOrder(), OrderEvent.Order);
                    break;
                }
                else Console.WriteLine(@"Input not valid");
            }
        }
    }
}
