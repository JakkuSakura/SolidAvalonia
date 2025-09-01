using Avalonia.Threading;

namespace SolidAvalonia;

/// <summary>
/// Base class for computations (memos and effects).
/// Computation is a ReactiveNode that can track dependencies and be re-executed when its dependencies change.
/// </summary>
internal abstract class Computation : IReactiveNode
{
    protected readonly ReactiveContext Context;
    protected readonly Scheduler Scheduler;
    protected readonly Dictionary<IReactiveNode, long> Dependencies = new();
    protected readonly List<Action> _cleanupActions = new();
    protected readonly List<IReactiveNode> _ownedNodes = new();
    protected readonly object SyncRoot = new();
    protected bool IsDirty = true;
    protected bool IsRunning;
    protected bool HasRun;
    
    public bool Disposed { get; protected set; }
    public long Version { get; protected set; }
    public IReactiveNode? Owner { get; private set; }

    protected Computation(ReactiveContext context, Scheduler scheduler)
    {
        Context = context;
        Scheduler = scheduler;
    }

    // Computations are reactive nodes that can own other nodes, track dependencies, and be re-executed
    public void AddOwnedNode(IReactiveNode node)
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            // Add to list but don't actually allow ownership
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

    // This method is kept for compatibility - internally we now use CleanupNode
    internal virtual void AddCleanup(Action cleanup)
    {
        if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));

        lock (SyncRoot)
        {
            _cleanupActions.Add(cleanup);
        }
    }

    protected void RunCleanup()
    {
        List<Action> cleanupActions;

        lock (SyncRoot)
        {
            if (_cleanupActions.Count == 0) return;
            cleanupActions = new List<Action>(_cleanupActions);
            _cleanupActions.Clear();
        }

        foreach (var cleanup in cleanupActions)
        {
            try
            {
                // Run on UI thread if available
                if (Dispatcher.UIThread?.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(cleanup);
                }
                else
                {
                    cleanup();
                }
            }
            catch (Exception ex)
            {
                // Log or handle cleanup errors
                Console.WriteLine($"Error in cleanup function: {ex}");
            }
        }
    }

    public void AddDependency(IReactiveNode node, long version)
    {
        lock (SyncRoot)
        {
            // Computations can be dependencies, but let's allow this now
            
            Dependencies[node] = version;
        }
    }

    public virtual void Invalidate()
    {
        lock (SyncRoot)
        {
            if (Disposed || IsDirty) return;
            IsDirty = true;
        }

        OnInvalidated();
    }

    protected virtual void OnInvalidated()
    {
        Scheduler.EnqueueComputation(this);
    }

    protected void ClearDependencies()
    {
        lock (SyncRoot)
        {
            foreach (var dep in Dependencies.Keys)
            {
                if (dep is Signal<object> signal)
                {
                    signal.RemoveObserver(this);
                }
            }

            Dependencies.Clear();
        }
    }

    public abstract void Execute();

    public virtual void Dispose()
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
            Disposed = true;

            // Run cleanup when computation is disposed
            RunCleanup();
            ClearDependencies();

            // Dispose all owned nodes
            var ownedNodesCopy = new List<IReactiveNode>(_ownedNodes);
            _ownedNodes.Clear();

            // Dispose owned nodes outside the lock
            foreach (var node in ownedNodesCopy)
            {
                node.Dispose();
            }

            // Remove from owner
            SetOwner(null);
        }
    }
    
    public void SetOwner(IReactiveNode? owner)
    {
        lock (SyncRoot)
        {
            if (Disposed) return;
                
            // Remove from old owner if exists
            Owner?.RemoveOwnedNode(this);
                
            // Set new owner
            Owner = owner;
                
            // Add to new owner if not null
            owner?.AddOwnedNode(this);
        }
    }
}