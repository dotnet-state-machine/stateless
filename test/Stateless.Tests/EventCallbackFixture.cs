using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xunit;

namespace Stateless.Tests
{

    public class EventCallbackFixture
    {
        [Fact]
        public async Task CanInvokeDelegate()
        {
            int count = 0;
            await EventCallbackFactory.Create(() => count = 1).InvokeAsync();

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CanInvokeDelegateOneArgument()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first) =>
            {
                count = new int[] { first, };
            }).InvokeAsync(1);

            Assert.Equal(new[] { 1, }, count);
        }

        [Fact]
        public async Task CanInvokeDelegateTwoArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second) =>
            {
                count = new int[] { first, second, };
            }).InvokeAsync(1, 2);

            Assert.Equal(new[] { 1, 2, }, count);
        }

        [Fact]
        public async Task CanInvokeDelegateThreeArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second, int third) =>
            {
                count = new int[] { first, second, third, };
            }).InvokeAsync(1, 2, 3);

            Assert.Equal(new[] { 1, 2, 3, }, count);
        }

        [Fact]
        public async Task CanInvokeDelegateFourArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second, int third, int four) =>
            {
                count = new int[] { first, second, third, four, };
            }).InvokeAsync(1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2, 3, 4, }, count);
        }

        [Fact]
        public async Task CanInvokeAsyncDelegate()
        {
            int count = 0;
            await EventCallbackFactory.Create(() => Task.Run(() => count = 1)).InvokeAsync();

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CanInvokeAsyncDelegateOneArgument()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first) =>
            {
                count = new int[] { first, };
            }).InvokeAsync(1);

            Assert.Equal(new[] { 1, }, count);
        }

        [Fact]
        public async Task CanInvokeAsyncDelegateTwoArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second) => Task.Run(() =>
            {
                count = new int[] { first, second, };
            })).InvokeAsync(1, 2);

            Assert.Equal(new[] { 1, 2, }, count);
        }

        [Fact]
        public async Task CanInvokeAsyncDelegateThreeArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second, int third) => Task.Run(() =>
            {
                count = new int[] { first, second, third, };
            })).InvokeAsync(1, 2, 3);

            Assert.Equal(new[] { 1, 2, 3, }, count);
        }

        [Fact]
        public async Task CanInvokeAsyncDelegateFourArguments()
        {
            int[] count = { };
            await EventCallbackFactory.Create((int first, int second, int third, int four) => Task.Run(() =>
            {
                count = new int[] { first, second, third, four, };
            })).InvokeAsync(1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2, 3, 4, }, count);
        }
    }
}
