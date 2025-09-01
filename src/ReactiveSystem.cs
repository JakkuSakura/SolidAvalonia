using System;
using System.Collections.Generic;

namespace SolidAvalonia;

/// <summary>
/// High-performance reactive system implementation with explicit dependency tracking
/// </summary>
internal class ReactiveSystem
{
    // Global singleton instance
    public static readonly ReactiveSystem Instance = new();

    internal readonly ReactiveContext Context = new();
    internal readonly Scheduler Scheduler = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    // Private constructor to prevent external instantiation
    private ReactiveSystem()
    {
    }


    #region IReactiveSystem Implementation

    /// <summary>
    /// Creates a reactive signal with getter and setter.
    /// Signal changes trigger updates to reactive UI.
    /// Signals can be called from anywhere, but must be created within a Component.
    /// </summary>
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        ThrowIfDisposed();
        var signal = new Signal<T>(initialValue);

        // Set owner to current reactive owner if available
        var currentOwner = Context.CurrentOwner;
        if (currentOwner == null)
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create a signal without an owner. Make sure signals are created within a reactive context (component).");
        }


        signal.SetOwner(currentOwner);

        return (signal.Get, signal.Set);
    }


    // CreateRef method removed - use CreateSignal instead

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    public Func<T> CreateMemo<T>(Func<T> computation)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(computation);

        var memo = new Memo<T>(computation, Context, Scheduler);

        // Set owner to current reactive owner if available
        var currentOwner = Context.CurrentOwner;
        if (currentOwner != null)
        {
            // Both Component and Computation can own Memos
            memo.SetOwner(currentOwner);
        }
        else
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create a memo without an owner. Make sure memos are created within a Component");
        }

        return memo.Get;
    }


    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    public void CreateEffect(Action effect)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(effect);

        var effectNode = new Effect(effect, Context, Scheduler);

        // Set owner to current reactive owner if available
        var currentOwner = Context.CurrentOwner;
        if (currentOwner == null)
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create an effect without an owner. Make sure effects are created within a Component");
        }

        // Both Component and Computation can own Effects
        effectNode.SetOwner(currentOwner);

        // Schedule initial execution
        Scheduler.EnqueueComputation(effectNode);
        Scheduler.ScheduleFlush();
    }
    
    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts. Cleanups are represented as leaf nodes.
    /// </summary>
    public void OnCleanup(Action cleanup)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(cleanup);

        var currentOwner = Context.CurrentOwner;
        if (currentOwner == null)
        {
            throw new InvalidOperationException(
                "OnCleanup must be called within a reactive context (component or effect)");
        }

        var cleanupNode = new CleanupNode(cleanup);
        currentOwner.AddOwnedNode(cleanupNode);
    }

    /// <summary>
    /// Pushes a cleanup owner onto the owner stack
    /// </summary>
    internal void PushOwner(IReactiveNode owner)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(owner);

        Context.Push(owner);
    }

    /// <summary>
    /// Pops the current cleanup owner from the owner stack
    /// </summary>
    internal IReactiveNode? PopOwner()
    {
        ThrowIfDisposed();
        return (IReactiveNode?)Context.Pop();
    }

    /// <summary>
    /// Runs a batch of updates, deferring effects until the end
    /// </summary>
    public void Batch(Action updates)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(updates);

        Context.IsBatching = true;
        try
        {
            updates();
        }
        finally
        {
            Context.IsBatching = false;
            Scheduler.ScheduleFlush();
        }
    }

    /// <summary>
    /// Disposes all resources used by the reactive system
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
        Scheduler.Clear();
        Context.Dispose();

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ReactiveSystem));
    }

    #endregion
}