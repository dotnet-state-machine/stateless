using System;
using System.Threading.Tasks;
using Stateless;

namespace AsyncExample
{
    public class Registration
    {
        private enum State
        {
            Start,
            WaitingForEmailConfirmation,
            WaitingForPhoneConfirmation,
            MailTokenNotFound,
            MailTokenAlreadyConfirmed,
            Complete
        }

        private enum Trigger
        {
            Register,
            ConfirmEmail,
            ConfirmPhone
        }

        private readonly StateMachine<State, Trigger> _machine;

        private readonly StateMachine<State, Trigger>.TriggerWithParameters<RegistrationForm> _registerTrigger;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string, string> _mailTokenTrigger;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<string, string> _smsTokenTrigger;

        public Registration()
        {
            _machine = new StateMachine<State, Trigger>(State.Start);

            _registerTrigger = _machine.SetTriggerParameters<RegistrationForm>(Trigger.Register);
            _mailTokenTrigger = _machine.SetTriggerParameters<string, string>(Trigger.ConfirmEmail);
            _smsTokenTrigger = _machine.SetTriggerParameters<string, string>(Trigger.ConfirmPhone);

            _machine
                .Configure(State.Start)
                .Permit(Trigger.Register, State.WaitingForEmailConfirmation);

            _machine
                .Configure(State.WaitingForEmailConfirmation)
                .OnEntryFromAsync(_registerTrigger, async registration => { await SendConfirmationMailAsync(registration); })
                .PermitIfAsync(_mailTokenTrigger,
                    guard: async (mailToken, _) => !await MailTokenValidAsync(mailToken),
                    destinationState: State.MailTokenNotFound)
                // Note that synchronous and asynchronous guards can be combined.
                .PermitIf(_mailTokenTrigger,
                    guard: (mailToken, _) => MailtokenAlreadyConfirmed(mailToken),
                    destinationState: State.MailTokenAlreadyConfirmed)
                .PermitIfAsync(_mailTokenTrigger,
                    guard: async (mailToken, _) => await MailTokenValidAsync(mailToken),
                    destinationState: State.WaitingForPhoneConfirmation);

            _machine
                .Configure(State.MailTokenNotFound)
                .OnEntry(() =>
                {
                    Console.WriteLine("ACTION: Mail token not found");
                });

            _machine
                .Configure(State.MailTokenAlreadyConfirmed)
                .OnEntry(() =>
                {
                    Console.WriteLine("ACTION: Mail token already confirmed");
                });

            _machine
                .Configure(State.WaitingForPhoneConfirmation)
                .OnEntryFromAsync(_mailTokenTrigger, async (mailToken, phone) => { await SendConfirmationSmsAsync(mailToken, phone); })
                .PermitIfAsync(_smsTokenTrigger,
                    guard: async (mailToken, smsToken) => await SmsTokenValidAsync(mailToken, smsToken),
                    destinationState: State.Complete)
                .PermitReentryIfAsync(_smsTokenTrigger,
                    guard: async (mailToken, smsToken) => !await SmsTokenValidAsync(mailToken, smsToken));

            _machine
                .Configure(State.Complete)
                .OnEntryFrom(_smsTokenTrigger, (mt, st) =>
                {
                    Console.WriteLine("ACTION: Storing registration in backend");
                });
        }

        public async Task Register(RegistrationForm registration)
        {
            await _machine.FireAsync(_registerTrigger, registration);
        }

        public async Task ConfirmEmail(string mailToken, string phone)
        {
            await _machine.FireAsync(_mailTokenTrigger, mailToken, phone);
        }

        public async Task ConfirmPhone(string mailToken, string smsToken)
        {
            await _machine.FireAsync(_smsTokenTrigger, mailToken, smsToken);
        }

        private async Task SendConfirmationMailAsync(RegistrationForm registration)
        {
            await Task.Delay(500);
            Console.WriteLine($"ACTION: Sending confirmation mail to {registration.Email}");
        }

        // Asynchronous guard.
        private async Task<bool> MailTokenValidAsync(string mailToken)
        {
            await Task.Delay(500);
            Console.WriteLine($"GUARD : Checking validity of mail token {mailToken}");
            return true;
        }

        // Synchronous guard.
        private bool MailtokenAlreadyConfirmed(string mailToken)
        {
            Console.WriteLine("GUARD : Checking whether mail token was already used");
            return false;
        }

        private async Task SendConfirmationSmsAsync(string mailToken, string phone)
        {
            await Task.Delay(500);
            Console.WriteLine($"ACTION: Sending confirmation SMS to {phone}");
        }

        // Asynchronous guard.
        private async Task<bool> SmsTokenValidAsync(string mailToken, string smsToken)
        {
            await Task.Delay(500);
            Console.WriteLine($"GUARD : Checking validity of SMS token {smsToken}");
            return true;
        }
    }
}