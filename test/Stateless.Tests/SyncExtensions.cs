namespace Stateless.Tests;

/// <summary>
/// </summary>
internal static class SyncExtensions {
    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public static void Fire<TState, TTrigger>(this StateMachine<TState, TTrigger> machine, TTrigger trigger)
        where TState : notnull where TTrigger : notnull {
        machine.FireAsync(trigger).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="args">A variable-length parameters list containing arguments. </param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public static void Fire<TState, TTrigger>(this StateMachine<TState, TTrigger> machine,
                                              TriggerWithParameters<TTrigger>     trigger, params object[] args)
        where TState : notnull where TTrigger : notnull {
        machine.FireAsync(trigger, args).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <param name="machine"></param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="arg0">The first argument.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public static void Fire<TState, TTrigger, TArg0>(this StateMachine<TState, TTrigger>    machine,
                                                     TriggerWithParameters<TTrigger, TArg0> trigger, TArg0 arg0)
        where TState : notnull where TTrigger : notnull {
        machine.FireAsync(trigger, arg0).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <param name="machine"></param>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public static void Fire<TState, TTrigger, TArg0, TArg1>(this StateMachine<TState, TTrigger> machine,
                                                            TriggerWithParameters<TTrigger, TArg0, TArg1> trigger,
                                                            TArg0 arg0, TArg1 arg1)
        where TState : notnull where TTrigger : notnull {
        machine.FireAsync(trigger, arg0, arg1).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
    /// <param name="machine"></param>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="arg2">The third argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public static void Fire<TState, TTrigger, TArg0, TArg1, TArg2>(this StateMachine<TState, TTrigger> machine,
                                                                   TriggerWithParameters<TTrigger, TArg0, TArg1, TArg2>
                                                                       trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        where TState : notnull where TTrigger : notnull {
        machine.FireAsync(trigger, arg0, arg1, arg2).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Activates current state. Actions associated with activating the current state
    ///     will be invoked. The activation is idempotent and subsequent activation of the same current state
    ///     will not lead to re-execution of activation callbacks.
    /// </summary>
    public static void Activate<TState, TTrigger>(this StateMachine<TState, TTrigger> machine)
        where TState : notnull where TTrigger : notnull {
        machine.ActivateAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Deactivates current state. Actions associated with deactivating the current state
    ///     will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state
    ///     will not lead to re-execution of deactivation callbacks.
    /// </summary>
    public static void Deactivate<TState, TTrigger>(this StateMachine<TState, TTrigger> machine)
        where TState : notnull where TTrigger : notnull {
        machine.DeactivateAsync().GetAwaiter().GetResult();
    }
}