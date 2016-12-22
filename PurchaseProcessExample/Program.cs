namespace PurchaseProcessExample
{
    class Program
    {
        static void Main(string[] args)
        {

            var bus = new Bus();

            var site = new Site();
            var shop = new Shop();
            var seller = new Seller();
            var sender = new Sender();

            site.Subscribe(bus);
            shop.Subscribe(bus);
            seller.Subscribe(bus);
            sender.Subscribe(bus);

            site.EnterNewOrder();
        }
    }
}
