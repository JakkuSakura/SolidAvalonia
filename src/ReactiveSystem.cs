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
    private readonly RootOwner _rootOwner;
    private bool _disposed;

    // Private constructor to prevent external instantiation
    private ReactiveSystem()
    {
        _rootOwner = new RootOwner(this);
    }


    /// <summary>
    /// Root owner for reactive nodes that manages lifecycle of top-level reactive nodes
    /// </summary>
    private class RootOwner : IReactiveOwner
    {
        private readonly ReactiveSystem _system;
        private readonly List<IDisposable> _disposables = new();
        private readonly List<ReactiveNode> _ownedNodes = new();

        public RootOwner(ReactiveSystem system)
        {
            _system = system;
        }

        public void AddCleanup(Action cleanup)
        {
            ArgumentNullException.ThrowIfNull(cleanup);
            // Convert cleanup action to IDisposable
            _disposables.Add(new ActionDisposable(cleanup));
        }

        public void AddOwnedNode(ReactiveNode node)
        {
            ArgumentNullException.ThrowIfNull(node);
            _ownedNodes.Add(node);
        }

        public void RemoveOwnedNode(ReactiveNode node)
        {
            ArgumentNullException.ThrowIfNull(node);
            _ownedNodes.Remove(node);
        }

        public void Dispose()
        {
            // Dispose all registered disposables
            foreach (var disposable in _disposables.ToArray())
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing cleanup action: {ex}");
                }
            }

            _disposables.Clear();

            // Dispose owned nodes
            foreach (var node in _ownedNodes.ToArray())
            {
                try
                {
                    node.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing root-owned node: {ex}");
                }
            }

            _ownedNodes.Clear();
        }

        // Cleanup method for backward compatibility
        public void Cleanup() => Dispose();
    }

    /// <summary>
    /// Simple disposable wrapper for an action
    /// </summary>
    private class ActionDisposable : IDisposable
    {
        private Action? _action;

        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            var action = _action;
            if (action != null)
            {
                _action = null;
                action();
            }
        }
    }


    #region IReactiveSystem Implementation

    /// <summary>
    /// Creates a reactive signal with getter and setter
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

        // Ensure the owner is a Component and not a Computation
        if (currentOwner is not Component)
            throw new InvalidOperationException(
                "Cannot create a signal with a Computation as owner. Signals must be owned by Components.");

        signal.SetOwner(currentOwner);

        return (signal.Get, signal.Set);
    }


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
                "Cannot create a memo without an owner. Make sure memos are created within a reactive context (component or computation)");
        }

        return memo.Get;
    }

    /// <summary>
    /// Creates a root-level computed value that automatically updates when dependencies change
    /// </summary>
    public Func<T> CreateRootMemo<T>(Func<T> computation)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(computation);

        var memo = new Memo<T>(computation, Context, Scheduler);
        memo.SetOwner(_rootOwner);
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
                "Cannot create an effect without an owner. Make sure effects are created within a reactive context (component or computation). Use CreateRootEffect for top-level effects.");
        }

        // Both Component and Computation can own Effects
        effectNode.SetOwner(currentOwner);

        // Schedule initial execution
        Scheduler.EnqueueComputation(effectNode);
        Scheduler.ScheduleFlush();
    }

    /// <summary>
    /// Creates a root-level effect that runs when dependencies change
    /// </summary>
    public void CreateRootEffect(Action effect)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(effect);

        var effectNode = new Effect(effect, Context, Scheduler);
        effectNode.SetOwner(_rootOwner);

        // Schedule initial execution
        Scheduler.EnqueueComputation(effectNode);
        Scheduler.ScheduleFlush();
    }


    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts
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

        currentOwner.AddCleanup(cleanup);
    }

    /// <summary>
    /// Pushes a cleanup owner onto the owner stack
    /// </summary>
    internal void PushOwner(IReactiveOwner owner)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(owner);

        Context.Push(owner);
    }

    /// <summary>
    /// Pops the current cleanup owner from the owner stack
    /// </summary>
    internal void PopOwner()
    {
        ThrowIfDisposed();
        Context.Pop<IReactiveOwner>();
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