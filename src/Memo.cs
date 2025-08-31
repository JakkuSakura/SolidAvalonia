namespace SolidAvalonia;

/// <summary>
/// Memo computation that caches its result
/// </summary>
internal class Memo<T> : Computation
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
            var current = ReactiveSystem.Instance.Context.CurrentComputation;
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