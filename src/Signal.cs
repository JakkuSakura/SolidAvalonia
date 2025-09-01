using System;
using System.Collections.Generic;

namespace SolidAvalonia;

/// <summary>
/// Represents a reactive signal that can be read and written.
/// Signals can be accessed from anywhere, tracking dependencies automatically.
/// </summary>
public class Signal<T> : IReactiveNode
{
    private T _value;
    private readonly HashSet<Computation> _observers = new();
    private readonly List<Action> _cleanupActions = new();
    private readonly List<IReactiveNode> _ownedNodes = new();
    protected readonly object SyncRoot = new();

    public bool Disposed { get; private set; }
    public long Version { get; private set; }
    public IReactiveNode? Owner { get; private set; }

    internal Signal(T initialValue)
    {
        _value = initialValue;
        Version = 0;
    }

    public T Get()
    {
        lock (SyncRoot)
        {
            // Register dependency if we're inside a computation
            var current = ReactiveSystem.Instance.Context.CurrentComputation;
            if (current is not { Disposed: false }) return _value;
            _observers.Add(current);
            current.AddDependency(this, Version);

            return _value;
        }
    }

    public void Set(T value)
    {
        HashSet<Computation>? observersToNotify = null;

        lock (SyncRoot)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;
            Version++;

            // Check if we have any observers to notify
            if (_observers.Count > 0)
            {
                var currentComputation = ReactiveSystem.Instance.Context.CurrentComputation;

                // Filter out the current computation to avoid redundant updates
                // Only necessary if we're inside a computation that depends on this signal
                if (currentComputation != null && _observers.Contains(currentComputation))
                {
                    // Create a new HashSet excluding the current computation
                    observersToNotify = new HashSet<Computation>(_observers);
                    observersToNotify.Remove(currentComputation);
                }
                else
                {
                    // No need to filter, notify all observers
                    observersToNotify = new HashSet<Computation>(_observers);
                }
            }
        }

        // Notify observers outside the lock to prevent deadlocks
        if (observersToNotify != null && observersToNotify.Count > 0)
        {
            foreach (var observer in observersToNotify)
            {
                observer.Invalidate();
            }

            // Only schedule flush if not in a batch
            if (!ReactiveSystem.Instance.Context.IsBatching)
            {
                ReactiveSystem.Instance.Scheduler.ScheduleFlush();
            }
        }
    }

    internal void RemoveObserver(Computation computation)
    {
        lock (SyncRoot)
        {
            _observers.Remove(computation);
        }
    }

    public void Dispose()
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            Disposed = true;
            _observers.Clear();
        }

        RunCleanup();

        // Remove from owner
        SetOwner(null);
    }

    // This method is kept for compatibility - internally we now use CleanupNode
    internal void AddCleanup(Action cleanup)
    {
        if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));

        lock (SyncRoot)
        {
            if (Disposed) return;
            _cleanupActions.Add(cleanup);
        }
    }

    public void AddOwnedNode(IReactiveNode node)
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            _ownedNodes.Add(node);
        }
    }

    public void RemoveOwnedNode(IReactiveNode node)
    {
        lock (SyncRoot)
        {
            _ownedNodes.Remove(node);
        }
    }

    public void SetOwner(IReactiveNode? owner)
    {
        if (owner is not Component)
            throw new ArgumentException("Owner must be a Component", nameof(owner));

        lock (SyncRoot)
        {
            if (Disposed) return;

            // Remove from old owner if exists
            Owner?.RemoveOwnedNode(this);

            // Set new owner
            Owner = owner;

            // Add to new owner if not null
            owner.AddOwnedNode(this);
        }
    }

    private void RunCleanup()
    {
        List<Action> cleanupActions;
        List<IReactiveNode> ownedNodes;

        lock (SyncRoot)
        {
            cleanupActions = new List<Action>(_cleanupActions);
            _cleanupActions.Clear();

            ownedNodes = new List<IReactiveNode>(_ownedNodes);
            _ownedNodes.Clear();
        }

        // Run all cleanup actions
        foreach (var cleanup in cleanupActions)
        {
            try
            {
                cleanup();
            }
            catch (Exception ex)
            {
                // Log the error but continue with other disposables
                Console.WriteLine($"Error in cleanup action: {ex}");
            }
        }

        // Dispose all owned nodes
        foreach (var node in ownedNodes)
        {
            try
            {
                node.Dispose();
            }
            catch (Exception ex)
            {
                // Log the error but continue with other nodes
                Console.WriteLine($"Error disposing reactive node: {ex}");
            }
        }
    }
}