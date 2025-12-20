using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests {
    /// <summary>
    /// We disable parallelization to ensure other threads are not occupied by other tests.
    /// Some tests regarding DropUnprocessedEventsOnErrorInSerialMode need to be added.
    /// </summary>
    [CollectionDefinition("SerialModeThreadSafetyFixture", DisableParallelization = true)]
    public class SerialModeThreadSafetyFixture {

        [Fact(Timeout = 60 * 1000)]
        public async Task IncrementIsThreadSafeUnderContentionSync() {

            var stateMachine = new StateMachine<State, Trigger>(State.A, FiringMode.Serial);

            int counter = 0;
            var startGate = new ManualResetEventSlim(false);

            stateMachine.Configure(State.A)
                .OnEntry(() => {
                    counter = counter + 1;
                })
                .PermitReentry(Trigger.X);

            var tasks = Enumerable.Range(0, 16).Select(_ =>
                Task.Run(() => {
                    startGate.Wait();
                    for (int i = 0; i < 10_000; i++) {
                        stateMachine.Fire(Trigger.X);
                    }
                })
            ).ToArray();

            startGate.Set();
            await Task.WhenAll(tasks);

            // Wait for the tasks to be done processing
            await stateMachine.GetSerialEventsWorkerTask();

            Assert.Equal(160_000, counter);

            stateMachine.Dispose();
        }

        [Fact(Timeout = 60 * 1000)]
        public async Task IncrementIsThreadSafeUnderContentionAsync() {

            var stateMachine = new StateMachine<State, Trigger>(State.A, FiringMode.Serial);

            int counter = 0;
            var startGate = new ManualResetEventSlim(false);

            stateMachine.Configure(State.A)
                .OnEntryAsync(async () => {
                    counter = counter + 1;
                })
                .PermitReentry(Trigger.X);

            var tasks = Enumerable.Range(0, 16).Select(_ =>
                Task.Run(async () => {
                    startGate.Wait();
                    for (int i = 0; i < 1_000; i++) {
                        await stateMachine.FireAsync(Trigger.X);
                    }
                })
            ).ToArray();

            startGate.Set();
            await Task.WhenAll(tasks);

            // Wait for the tasks to be done processing
            await stateMachine.GetSerialEventsWorkerTask();

            Assert.Equal(16_000, counter);

            stateMachine.Dispose();
        }

        [Fact(Timeout = 60 * 1000)]
        public async Task RecursiveFireDoesNotDeadlockSync() {

            var stateMachine = new StateMachine<State, Trigger>(State.A, FiringMode.Serial);

            stateMachine.Configure(State.A)
                .OnEntry(() => {
                    stateMachine.Fire(Trigger.Y);
                })
                .Permit(Trigger.Y, State.B)
                .PermitReentry(Trigger.X);

            stateMachine.Configure(State.B)
                .OnEntry(() => {
                    stateMachine.Fire(Trigger.Y);
                })
                .Permit(Trigger.Y, State.C)
                .PermitReentry(Trigger.X);

            stateMachine.Configure(State.C);

            stateMachine.Fire(Trigger.X);

            // Wait for the tasks to be done processing
            await stateMachine.GetSerialEventsWorkerTask();

            Assert.Equal(State.C, stateMachine.State);

            stateMachine.Dispose();
        }

        [Fact(Timeout = 60*1000)]
        public async Task RecursiveFireDoesNotDeadlockAsync() {

            var stateMachine = new StateMachine<State, Trigger>(State.A, FiringMode.Serial);

            stateMachine.Configure(State.A)
                .OnEntryAsync(async () => {
                    await stateMachine.FireAsync(Trigger.Y);
                })
                .Permit(Trigger.Y, State.B)
                .PermitReentry(Trigger.X);

            stateMachine.Configure(State.B)
                .OnEntryAsync(async () => {
                    await stateMachine.FireAsync(Trigger.Y);
                })
                .Permit(Trigger.Y, State.C)
                .PermitReentry(Trigger.X);

            stateMachine.Configure(State.C);

            await stateMachine.FireAsync(Trigger.X);

            // Wait for the tasks to be done processing
            await stateMachine.GetSerialEventsWorkerTask();

            Assert.Equal(State.C, stateMachine.State);

            stateMachine.Dispose();
        }

    }
}
