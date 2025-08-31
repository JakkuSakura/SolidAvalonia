namespace SolidAvalonia;

/// <summary>
/// Represents a reactive signal that can be read and written
/// </summary>
public class Signal<T> : ReactiveNode
{
    private T _value;
    private readonly HashSet<ReactiveSystem.Computation> _observers = new();
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
            var current = ReactiveSystem.Instance._context.CurrentComputation;
            if (current is not { Disposed: false }) return _value;
            _observers.Add(current);
            current.AddDependency(this, Version);

            return _value;
        }
    }

    public void Set(T value)
    {
        HashSet<ReactiveSystem.Computation>? observersToNotify = null;

        lock (SyncRoot)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;
            Version++;

            if (_observers.Count > 0)
            {
                observersToNotify = new HashSet<ReactiveSystem.Computation>(_observers);
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
            if (!ReactiveSystem.Instance._context.IsBatching)
            {
                ReactiveSystem.Instance._scheduler.ScheduleFlush();
            }
        }
    }

    internal void RemoveObserver(ReactiveSystem.Computation computation)
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