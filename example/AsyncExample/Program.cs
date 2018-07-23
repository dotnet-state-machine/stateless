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
            var emailConfirmationResult = await registration.ConfirmEmail("qwerty_mail_token", phone);
            if (phone != null && emailConfirmationResult == Registration.EmailConfirmationResult.Success)
            {
                // Phone number should only be confirmed when there actually is a phone number and when
                // email was successfully confirmed.
                await registration.ConfirmPhone("qwerty_mail_token", "asdfgh_sms_token");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
