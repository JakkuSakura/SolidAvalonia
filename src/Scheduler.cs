using Avalonia.Threading;

namespace SolidAvalonia;

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