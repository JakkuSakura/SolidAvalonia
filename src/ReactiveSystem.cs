using Avalonia.Threading;

namespace SolidAvalonia;

/// <summary>
/// High-performance reactive system implementation with explicit dependency tracking
/// </summary>
internal class ReactiveSystem
{
    // Global singleton instance
    public static readonly ReactiveSystem Instance = new();

    internal readonly ReactiveContext _context = new();
    internal Component? CurrentComponent = null;
    internal readonly Scheduler _scheduler = new();
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
    private class RootOwner : IReactiveOwner, IDisposable
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
            if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));
            // Convert cleanup action to IDisposable
            _disposables.Add(new ActionDisposable(cleanup));
        }

        public void AddOwnedNode(ReactiveNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            _ownedNodes.Add(node);
        }

        public void RemoveOwnedNode(ReactiveNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
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

    #region Core Types


    /// <summary>
    /// Base class for computations (memos and effects)
    /// </summary>
    internal abstract class Computation : ReactiveNode, IReactiveOwner
    {
        protected readonly ReactiveContext Context;
        protected readonly Scheduler Scheduler;
        protected readonly Dictionary<ReactiveNode, long> Dependencies = new();
        protected readonly List<Action> _cleanupActions = new();
        protected readonly List<ReactiveNode> _ownedNodes = new();
        protected bool IsDirty = true;
        protected bool IsRunning;
        protected bool HasRun;

        protected Computation(ReactiveContext context, Scheduler scheduler)
        {
            Context = context;
            Scheduler = scheduler;
        }

        public void AddOwnedNode(ReactiveNode node)
        {
            lock (SyncRoot)
            {
                if (Disposed) return;
                _ownedNodes.Add(node);
            }
        }

        public void RemoveOwnedNode(ReactiveNode node)
        {
            lock (SyncRoot)
            {
                _ownedNodes.Remove(node);
            }
        }

        public virtual void AddCleanup(Action cleanup)
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

        public void AddDependency(ReactiveNode node, long version)
        {
            lock (SyncRoot)
            {
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

        public override void Dispose()
        {
            lock (SyncRoot)
            {
                if (Disposed) return;
                Disposed = true;

                // Run cleanup when computation is disposed
                RunCleanup();
                ClearDependencies();

                // Dispose all owned nodes
                var ownedNodesCopy = new List<ReactiveNode>(_ownedNodes);
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
    }

    /// <summary>
    /// Memo computation that caches its result
    /// </summary>
    private class Memo<T> : Computation
    {
        private readonly Func<T> _computation;
        private T _value;
        private bool _hasValue;
        private readonly HashSet<Computation> _observers = new();

        public Memo(Func<T> computation, ReactiveContext context, Scheduler scheduler)
            : base(context, scheduler)
        {
            _computation = computation;
            _value = default!;
        }

        public T Get()
        {
            lock (SyncRoot)
            {
                // Register as dependency if we're in a computation
                var current = Context.CurrentComputation;
                if (current != null && !current.Disposed)
                {
                    _observers.Add(current);
                    current.AddDependency(this, Version);
                }

                // Compute if dirty or first run
                if (IsDirty || !_hasValue)
                {
                    Execute();
                }

                return _value;
            }
        }

        public override void Execute()
        {
            if (IsRunning) return; // Prevent circular dependencies

            lock (SyncRoot)
            {
                if (!IsDirty && _hasValue) return;
                IsRunning = true;
            }

            try
            {
                // Clear old dependencies
                ClearDependencies();

                // Track this computation
                Context.Push(this);

                T newValue;
                try
                {
                    newValue = _computation();
                }
                finally
                {
                    Context.Pop();
                }

                lock (SyncRoot)
                {
                    var changed = !_hasValue || !EqualityComparer<T>.Default.Equals(_value, newValue);
                    _value = newValue;
                    _hasValue = true;
                    IsDirty = false;
                    HasRun = true;

                    if (changed)
                    {
                        Version++;

                        // Notify dependent computations
                        foreach (var observer in _observers.ToList())
                        {
                            observer.Invalidate();
                        }
                    }
                }
            }
            finally
            {
                lock (SyncRoot)
                {
                    IsRunning = false;
                }
            }
        }

        protected override void OnInvalidated()
        {
            // Memos are lazy - don't schedule execution until read
            lock (SyncRoot)
            {
                // Propagate invalidation to observers
                foreach (var observer in _observers.ToList())
                {
                    observer.Invalidate();
                }
            }
        }

        public override void Dispose()
        {
            lock (SyncRoot)
            {
                _observers.Clear();
            }

            base.Dispose();
        }
    }

    /// <summary>
    /// Effect computation that runs side effects
    /// </summary>
    private class Effect : Computation
    {
        private readonly Action _effect;
        private static readonly ThreadLocal<Effect> _currentEffect = new();

        public Effect(Action effect, ReactiveContext context, Scheduler scheduler)
            : base(context, scheduler)
        {
            _effect = effect;
            // Effects start dirty and need to be scheduled
            IsDirty = true;
        }


        public override void Execute()
        {
            if (IsRunning) return;

            lock (SyncRoot)
            {
                if (Disposed) return;
                IsRunning = true;
            }

            try
            {
                // Run cleanup functions before re-running the effect
                if (HasRun)
                {
                    RunCleanup();
                }

                // Clear old dependencies
                ClearDependencies();

                // Track this computation and set as current effect
                Context.Push(this);
                var previousEffect = _currentEffect.Value;
                _currentEffect.Value = this;

                // Push this effect as current owner for cleanup registration
                Instance._context.Push(this);

                try
                {
                    // Run on UI thread if available
                    if (Dispatcher.UIThread?.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => { _effect(); });
                    }
                    else
                    {
                        _effect();
                    }

                    // Note: We can't capture return values from actions directly in C#
                    // Instead, we rely on explicit OnCleanup calls inside effects
                }
                finally
                {
                    // Using the null-forgiving operator to suppress the warning,
                    // since we're restoring the previous state
                    _currentEffect.Value = previousEffect!;
                    Context.Pop();

                    // Pop this effect as current owner
                    Instance._context.Pop<IReactiveOwner>();
                }

                lock (SyncRoot)
                {
                    IsDirty = false;
                    HasRun = true;
                    Version++;
                }
            }
            finally
            {
                lock (SyncRoot)
                {
                    IsRunning = false;
                }
            }
        }

        protected override void OnInvalidated()
        {
            // Effects are eager - schedule execution immediately
            base.OnInvalidated();

            if (!Context.IsBatching)
            {
                Scheduler.ScheduleFlush();
            }
        }

        public override void Dispose()
        {
            // Run cleanup when the effect is disposed (component unmounts)
            RunCleanup();
            base.Dispose();
        }

        public static Effect? Current => _currentEffect.Value;
    }

    /// <summary>
    /// Schedules and batches computation updates
    /// </summary>
    internal class Scheduler
    {
        private readonly Queue<Computation> _pendingComputations = new();
        private readonly HashSet<Computation> _enqueuedComputations = new();
        private readonly object _lock = new();
        private bool _isFlushScheduled;
        private bool _isFlushing;

        public void EnqueueComputation(Computation computation)
        {
            lock (_lock)
            {
                if (_enqueuedComputations.Contains(computation))
                    return;

                _pendingComputations.Enqueue(computation);
                _enqueuedComputations.Add(computation);
            }
        }

        public void ScheduleFlush()
        {
            lock (_lock)
            {
                if (_isFlushScheduled || _isFlushing)
                    return;

                _isFlushScheduled = true;
            }

            // Schedule on next frame/tick
            if (Dispatcher.UIThread != null)
            {
                Dispatcher.UIThread.Post(Flush, DispatcherPriority.Normal);
            }
            else
            {
                // Fallback to thread pool if no UI thread
                ThreadPool.QueueUserWorkItem(_ => Flush());
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                if (_isFlushing) return;
                _isFlushing = true;
                _isFlushScheduled = false;
            }

            try
            {
                while (true)
                {
                    Computation? computation;

                    lock (_lock)
                    {
                        if (_pendingComputations.Count == 0)
                            break;

                        computation = _pendingComputations.Dequeue();
                        _enqueuedComputations.Remove(computation);
                    }

                    if (!computation.Disposed)
                    {
                        computation.Execute();
                    }
                }
            }
            finally
            {
                lock (_lock)
                {
                    _isFlushing = false;

                    // If new computations were added during flush, schedule another flush
                    if (_pendingComputations.Count > 0)
                    {
                        ScheduleFlush();
                    }
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pendingComputations.Clear();
                _enqueuedComputations.Clear();
            }
        }
    }

    #endregion

    #region IReactiveSystem Implementation

    /// <summary>
    /// Creates a reactive signal with getter and setter
    /// </summary>
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        ThrowIfDisposed();
        var signal = new Signal<T>(initialValue);

        // Set owner to current reactive owner if available
        var currentOwner = _context.CurrentOwner;
        if (currentOwner != null)
        {
            signal.SetOwner(currentOwner);
        }
        else
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create a signal without an owner. Make sure signals are created within a reactive context (component or effect).");
        }

        return (signal.Get, signal.Set);
    }
    


    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    public Func<T> CreateMemo<T>(Func<T> computation)
    {
        ThrowIfDisposed();
        if (computation == null) throw new ArgumentNullException(nameof(computation));

        var memo = new Memo<T>(computation, _context, _scheduler);

        // Set owner to current reactive owner if available
        var currentOwner = _context.CurrentOwner;
        if (currentOwner != null)
        {
            memo.SetOwner(currentOwner);
        }
        else
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create a memo without an owner. Make sure memos are created within a reactive context (component or effect)");
        }

        return memo.Get;
    }

    /// <summary>
    /// Creates a root-level computed value that automatically updates when dependencies change
    /// </summary>
    public Func<T> CreateRootMemo<T>(Func<T> computation)
    {
        ThrowIfDisposed();
        if (computation == null) throw new ArgumentNullException(nameof(computation));

        var memo = new Memo<T>(computation, _context, _scheduler);
        memo.SetOwner(_rootOwner);
        return memo.Get;
    }

    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    public void CreateEffect(Action effect)
    {
        ThrowIfDisposed();
        if (effect == null) throw new ArgumentNullException(nameof(effect));

        var effectNode = new Effect(effect, _context, _scheduler);

        // Set owner to current reactive owner if available
        var currentOwner = _context.CurrentOwner;
        if (currentOwner != null)
        {
            effectNode.SetOwner(currentOwner);
        }
        else
        {
            // Throw exception if no owner is available
            throw new InvalidOperationException(
                "Cannot create an effect without an owner. Make sure effects are created within a reactive context (component or other effect). Use CreateRootEffect for top-level effects.");
        }

        // Schedule initial execution
        _scheduler.EnqueueComputation(effectNode);
        _scheduler.ScheduleFlush();
    }
    
    /// <summary>
    /// Creates a root-level effect that runs when dependencies change
    /// </summary>
    public void CreateRootEffect(Action effect)
    {
        ThrowIfDisposed();
        if (effect == null) throw new ArgumentNullException(nameof(effect));

        var effectNode = new Effect(effect, _context, _scheduler);
        effectNode.SetOwner(_rootOwner);
        
        // Schedule initial execution
        _scheduler.EnqueueComputation(effectNode);
        _scheduler.ScheduleFlush();
    }


    /// <summary>
    /// Registers a cleanup function to be called before the current effect re-runs
    /// or when the component unmounts
    /// </summary>
    public void OnCleanup(Action cleanup)
    {
        ThrowIfDisposed();
        if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));

        var currentOwner = _context.CurrentOwner;
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
        if (owner == null) throw new ArgumentNullException(nameof(owner));

        _context.Push(owner);
    }

    /// <summary>
    /// Pops the current cleanup owner from the owner stack
    /// </summary>
    internal void PopOwner()
    {
        ThrowIfDisposed();
        _context.Pop<IReactiveOwner>();
    }

    /// <summary>
    /// Runs a batch of updates, deferring effects until the end
    /// </summary>
    public void Batch(Action updates)
    {
        ThrowIfDisposed();
        if (updates == null) throw new ArgumentNullException(nameof(updates));

        _context.IsBatching = true;
        try
        {
            updates();
        }
        finally
        {
            _context.IsBatching = false;
            _scheduler.ScheduleFlush();
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
        _scheduler.Clear();
        _context.Dispose();

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ReactiveSystem));
    }

    #endregion
}