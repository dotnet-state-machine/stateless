using System;
using System.Runtime.CompilerServices;
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

        public enum EmailConfirmationResult
        {
            Success,
            EmailTokenNotFound,
            EmailAlreadyConfirmed
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
                .OnAsyncEntryFrom(_registerTrigger, async registration => { await SendConfirmationMailAsync(registration); })
                // Demonstration of an asynchronous state selector.
                .PermitAsyncDynamicIf(_mailTokenTrigger,
                    async (mailToken, _) => await MailTokenStateSelectorAsync(mailToken));

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
                .OnAsyncEntryFrom(_mailTokenTrigger, async (mailToken, phone) => { await SendConfirmationSmsAsync(mailToken, phone); })
                .PermitAsyncIf(_smsTokenTrigger,
                    guard: async (mailToken, smsToken) => await SmsTokenValidAsync(mailToken, smsToken),
                    destinationState: State.Complete)
                .PermitAsyncReentryIf(_smsTokenTrigger,
                    guard: async (mailToken, smsToken) => !await SmsTokenValidAsync(mailToken, smsToken));

            _machine
                .Configure(State.Complete)
                .OnEntryFrom(_smsTokenTrigger, (mt, st) =>
                {
                    Console.WriteLine("ACTION: Storing registration in backend");
                });
        }

        private async Task<State> MailTokenStateSelectorAsync(string mailToken)
        {
            if (MailtokenAlreadyConfirmed(mailToken))
            {
                return State.MailTokenAlreadyConfirmed;
            }
            if (await MailTokenValidAsync(mailToken))
            {
                return State.WaitingForPhoneConfirmation;
            }
            return State.MailTokenNotFound;
        }

        public async Task Register(RegistrationForm registration)
        {
            await _machine.FireAsync(_registerTrigger, registration);
        }

        public async Task<EmailConfirmationResult> ConfirmEmail(string mailToken, string phone)
        {
            await _machine.FireAsync(_mailTokenTrigger, mailToken, phone);

            var machineState = _machine.State;
            if (machineState == State.MailTokenNotFound)
            {
                return EmailConfirmationResult.EmailTokenNotFound;
            }

            if (machineState == State.MailTokenAlreadyConfirmed)
            {
                return EmailConfirmationResult.EmailAlreadyConfirmed;
            }

            return EmailConfirmationResult.Success;
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