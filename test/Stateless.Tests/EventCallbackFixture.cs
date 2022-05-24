namespace Stateless.Tests;

public class EventCallbackFixture {
    [Fact]
    public async Task CanInvokeAsyncDelegate() {
        var count = 0;
        await EventCallbackFactory.Create(() => Task.Run(() => count = 1)).InvokeAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CanInvokeAsyncDelegateOneArgument() {
        var count = Array.Empty<int>();
        await EventCallbackFactory.Create((int first) => { count = new[] { first }; }).InvokeAsync(1);

        Assert.Equal(new[] { 1 }, count);
    }

    [Fact]
    public async Task CanInvokeAsyncDelegateThreeArguments() {
        var count = Array.Empty<int>();
        await EventCallbackFactory
             .Create((int first, int second, int third) => Task.Run(() => { count = new[] { first, second, third }; }))
             .InvokeAsync(1, 2, 3);

        Assert.Equal(new[] { 1, 2, 3 }, count);
    }

    [Fact]
    public async Task CanInvokeAsyncDelegateTwoArguments() {
        var count = Array.Empty<int>();
        await EventCallbackFactory
             .Create((int first, int second) => Task.Run(() => { count = new[] { first, second }; })).InvokeAsync(1, 2);

        Assert.Equal(new[] { 1, 2 }, count);
    }

    [Fact]
    public async Task CanInvokeDelegate() {
        var count = 0;
        await EventCallbackFactory.Create(() => count = 1).InvokeAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CanInvokeDelegateOneArgument() {
        var count = Array.Empty<int>();
        await EventCallbackFactory.Create((int first) => { count = new[] { first }; }).InvokeAsync(1);

        Assert.Equal(new[] { 1 }, count);
    }

    [Fact]
    public async Task CanInvokeDelegateThreeArguments() {
        var count = Array.Empty<int>();
        await EventCallbackFactory.Create((int first, int second, int third) => {
            count = new[] { first, second, third };
        }).InvokeAsync(1, 2, 3);

        Assert.Equal(new[] { 1, 2, 3 }, count);
    }

    [Fact]
    public async Task CanInvokeDelegateTwoArguments() {
        var count = Array.Empty<int>();
        await EventCallbackFactory.Create((int first, int second) => { count = new[] { first, second }; })
                                  .InvokeAsync(1, 2);

        Assert.Equal(new[] { 1, 2 }, count);
    }
}