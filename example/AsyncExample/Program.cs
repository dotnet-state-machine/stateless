using System;
using System.Threading.Tasks;

namespace AsyncExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string email = "rwwilden@gmail.com";
            const string phone = "+31612345678";

            var registration = new Registration();
            await registration.Register(new RegistrationForm { Email = email, Phone = phone });
            await registration.ConfirmEmail("qwerty_mail_token", phone);
            await registration.ConfirmPhone("qwerty_mail_token", "asdfgh_sms_token");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
