using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Stateless.Tests;

public class SynchronizationContextFixture
{
    private readonly MaxConcurrencySyncContext _syncContext = new(3);
    private readonly List<SynchronizationContext> _capturedSyncContext = new();
    
    private StateMachine<State, Trigger> GetSut(State initialState = State.A)
    {
        var sm = new StateMachine<State, Trigger>(initialState, FiringMode.Queued);
        sm.RetainSynchronizationContext = true;
        return sm;
    }
    
    private void SetSyncContext()
    {
        SynchronizationContext.SetSynchronizationContext(_syncContext);
    }
    
    private async Task CaptureThenLoseSyncContext()
    {
        CaptureSyncContext();
        await LoseSyncContext().ConfigureAwait(false);
    }
    
    private void CaptureSyncContext()
    {
        _capturedSyncContext.Add(SynchronizationContext.Current);
    }

    private async Task LoseSyncContext()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
        Assert.NotEqual(_syncContext, SynchronizationContext.Current);
    }

    private void AssertSyncContextAlwaysRetained(int? numberOfExpectedCalls = null)
    {
        Assert.NotEmpty(_capturedSyncContext);
        if (numberOfExpectedCalls is not null)
            Assert.Equal(numberOfExpectedCalls, _capturedSyncContext.Count);
        
        Assert.All(_capturedSyncContext, actual => Assert.Equal(_syncContext, actual));
    }

    [Fact]
    public void Ensure_XUnit_is_using_SyncContext()
    {
        SetSyncContext();
        CaptureSyncContext();
        AssertSyncContextAlwaysRetained();
    }
    
    [Fact]
    public async Task Ensure_XUnit_can_lose_sync_context()
    {
        SetSyncContext();
        await LoseSyncContext().ConfigureAwait(false);
        Assert.NotEqual(_syncContext, SynchronizationContext.Current);
    }

    [Fact]
    public async Task Single_activation_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .OnActivateAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.ActivateAsync();
        
        // ASSERT
        AssertSyncContextAlwaysRetained(1);
    }
    
    [Fact]
    public async Task Activation_of_state_with_superstate_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .OnActivateAsync(CaptureThenLoseSyncContext)
            .SubstateOf(State.B);
            ;
        sm.Configure(State.B)
            .OnActivateAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.ActivateAsync();
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task Deactivation_of_state_with_superstate_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .OnDeactivateAsync(CaptureThenLoseSyncContext)
            .SubstateOf(State.B);
            ;
        sm.Configure(State.B)
            .OnDeactivateAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.DeactivateAsync();
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task Multiple_activations_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .OnActivateAsync(CaptureThenLoseSyncContext)
            .OnActivateAsync(CaptureThenLoseSyncContext)
            .OnActivateAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.ActivateAsync();
        
        // ASSERT
        AssertSyncContextAlwaysRetained(3);
    }
    
    [Fact]
    public async Task Multiple_Deactivations_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .OnDeactivateAsync(CaptureThenLoseSyncContext)
            .OnDeactivateAsync(CaptureThenLoseSyncContext)
            .OnDeactivateAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.DeactivateAsync();
        
        // ASSERT
        AssertSyncContextAlwaysRetained(3);
    }
    
    [Fact]
    public async Task OnEntry_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A).Permit(Trigger.X, State.B);
        sm.Configure(State.B)
            .OnEntryAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(1);
    }

    [Fact]
    public async Task Multiple_OnEntry_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A).Permit(Trigger.X, State.B);
        sm.Configure(State.B)
            .OnEntryAsync(CaptureThenLoseSyncContext)
            .OnEntryAsync(CaptureThenLoseSyncContext)
            .OnEntryAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(3);
    } 
    
    [Fact]
    public async Task Multiple_OnExit_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .Permit(Trigger.X, State.B)
            .OnExitAsync(CaptureThenLoseSyncContext)
            .OnExitAsync(CaptureThenLoseSyncContext)
            .OnExitAsync(CaptureThenLoseSyncContext);
        sm.Configure(State.B);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(3);
    }
    
    [Fact]
    public async Task OnExit_state_with_superstate_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut(State.B);
        sm.Configure(State.A)
            .OnExitAsync(CaptureThenLoseSyncContext)
            ;
        
        sm.Configure(State.B)
            .SubstateOf(State.A)
            .Permit(Trigger.X, State.C)
            .OnExitAsync(CaptureThenLoseSyncContext)
            ;
        sm.Configure(State.C);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task OnExit_state_and_superstate_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut(State.C);
        sm.Configure(State.A);

        sm.Configure(State.B)
            .SubstateOf(State.A)
            .OnExitAsync(CaptureThenLoseSyncContext);
        
        sm.Configure(State.C)
            .SubstateOf(State.B)
            .Permit(Trigger.X, State.A)
            .OnExitAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task Multiple_OnEntry_on_Reentry_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A).PermitReentry(Trigger.X)
            .OnEntryAsync(CaptureThenLoseSyncContext)
            .OnEntryAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task Multiple_OnExit_on_Reentry_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A).PermitReentry(Trigger.X)
            .OnExitAsync(CaptureThenLoseSyncContext)
            .OnExitAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }

    [Fact]
    public async Task Trigger_firing_another_Trigger_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .InternalTransitionAsync(Trigger.X, async () =>
            {
                await CaptureThenLoseSyncContext();
                await sm.FireAsync(Trigger.Y);
            })
            .Permit(Trigger.Y, State.B)
            ;
        sm.Configure(State.B)
            .OnEntryAsync(CaptureThenLoseSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(2);
    }
    
    [Fact]
    public async Task OnTransition_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .Permit(Trigger.X, State.B);

        sm.Configure(State.B);

        sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
        sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
        sm.OnTransitionedAsync(_ => CaptureThenLoseSyncContext());
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(3);
    }
    
    [Fact]
    public async Task InternalTransition_firing_a_sync_action_should_retain_SyncContext()
    {
        // ARRANGE
        SetSyncContext();
        var sm = GetSut();
        sm.Configure(State.A)
            .InternalTransition(Trigger.X, CaptureSyncContext);
        
        // ACT
        await sm.FireAsync(Trigger.X);
        
        // ASSERT
        AssertSyncContextAlwaysRetained(1);
    }
}