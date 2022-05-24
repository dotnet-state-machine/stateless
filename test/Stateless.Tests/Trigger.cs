namespace Stateless.Tests;

internal enum Trigger {
    X,
    Y,
    Z
}

internal class FirstFakeTrigger {
    public bool IsAllowed { get; set; }
}

internal class SecondFakeTrigger {
    public bool IsOk { get; set; }
}