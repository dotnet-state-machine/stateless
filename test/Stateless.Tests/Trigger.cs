﻿namespace Stateless.Tests
{
    enum Trigger
    {
        X, Y, Z
    }
    class FirstFakeTrigger
    {
        public bool IsAllowed { get; set; }
    }



    class SecondFakeTrigger
    {
        public bool IsOk { get; set; }
    }
}
