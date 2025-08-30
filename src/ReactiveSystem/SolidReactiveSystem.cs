using Avalonia.Threading;

namespace SolidAvalonia.ReactiveSystem;

/// <summary>
/// High-performance reactive system implementation with explicit dependency tracking
/// </summary>
internal class SolidReactiveSystem : IReactiveSystem
{
    private readonly ComputationContext _context = new();
    private readonly Scheduler _scheduler = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    // Private constructor to prevent external instantiation
    internal SolidReactiveSystem()
    {
    }


    #region Core Types

    /// <summary>
    /// Tracks the currently executing computation for dependency registration
    /// </summary>
    private class ComputationContext : IDisposable
    {
        private readonly Stack<Computation> _computationStack = new();

        public Computation? Current => _computationStack.Count > 0 ? _computationStack.Peek() : null;
        public bool IsBatching;

        public void Push(Computation computation) => _computationStack.Push(computation);

        public void Pop()
        {
            if (_computationStack.Count > 0) _computationStack.Pop();
        }

        public void Clear() => _computationStack.Clear();
        public void Dispose() => Clear();
    }

    /// <summary>
    /// Base class for all reactive nodes in the dependency graph
    /// </summary>
    private abstract class ReactiveNode : IDisposable
    {
        protected readonly object SyncRoot = new();
        public bool Disposed;
        public long Version { get; protected set; }
        public abstract void Dispose();
    }

    /// <summary>
    /// Represents a reactive signal that can be read and written
    /// </summary>
    private class Signal<T> : ReactiveNode
    {
        private T _value;
        private readonly HashSet<Computation> _observers = new();
        private readonly ComputationContext _context;
        private readonly Scheduler _scheduler;

        public Signal(T initialValue, ComputationContext context, Scheduler scheduler)
        {
            _value = initialValue;
            _context = context;
            _scheduler = scheduler;
            Version = 0;
        }

        public T Get()
        {
            lock (SyncRoot)
            {
                // Register dependency if we're inside a computation
                var current = _context.Current;
                if (current != null && !current.Disposed)
                {
                    _observers.Add(current);
                    current.AddDependency(this, Version);
                }

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

                if (_observers.Count > 0)
                {
                    observersToNotify = new HashSet<Computation>(_observers);
                }
            }

            // Notify observers outside the lock to prevent deadlocks
            if (observersToNotify != null)
            {
                foreach (var observer in observersToNotify)
                {
                    observer.Invalidate();
                }

                // Only schedule flush if not in a batch
                if (!_context.IsBatching)
                {
                    _scheduler.ScheduleFlush();
                }
            }
        }

        public void RemoveObserver(Computation computation)
        {
            lock (SyncRoot)
            {
                _observers.Remove(computation);
            }
        }

        public override void Dispose()
        {
            lock (SyncRoot)
            {
                if (Disposed) return;
                Disposed = true;
                _observers.Clear();
            }
        }
    }

    /// <summary>
    /// Base class for computations (memos and effects)
    /// </summary>
    private abstract class Computation : ReactiveNode
    {
        protected readonly ComputationContext Context;
        protected readonly Scheduler Scheduler;
        protected readonly Dictionary<ReactiveNode, long> Dependencies = new();
        protected bool IsDirty = true;
        protected bool IsRunning;
        protected bool HasRun;

        protected Computation(ComputationContext context, Scheduler scheduler)
        {
            Context = context;
            Scheduler = scheduler;
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
                ClearDependencies();
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

        public Memo(Func<T> computation, ComputationContext context, Scheduler scheduler)
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
                var current = Context.Current;
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
        private readonly List<Action> _cleanupActions = new();
        private static readonly ThreadLocal<Effect> _currentEffect = new();

        public Effect(Action effect, ComputationContext context, Scheduler scheduler)
            : base(context, scheduler)
        {
            _effect = effect;
            // Effects start dirty and need to be scheduled
            IsDirty = true;
        }

        public void AddCleanup(Action cleanup)
        {
            if (cleanup == null) throw new ArgumentNullException(nameof(cleanup));
            
            lock (SyncRoot)
            {
                _cleanupActions.Add(cleanup);
            }
        }

        private void RunCleanup()
        {
            List<Action> cleanupActions;
            
            lock (SyncRoot)
            {
                if (_cleanupActions.Count == 0) return;
                cleanupActions = new List<Action>(_cleanupActions);
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
            
            lock (SyncRoot)
            {
                _cleanupActions.Clear();
            }
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
    private class Scheduler
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
        var signal = new Signal<T>(initialValue, _context, _scheduler);
        _disposables.Add(signal);
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
        _disposables.Add(memo);
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
        _disposables.Add(effectNode);

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
        
        var currentEffect = Effect.Current;
        if (currentEffect == null)
        {
            throw new InvalidOperationException("OnCleanup must be called within an effect");
        }
        
        currentEffect.AddCleanup(cleanup);
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
        if (_disposed) throw new ObjectDisposedException(nameof(SolidReactiveSystem));
    }

    #endregion
}