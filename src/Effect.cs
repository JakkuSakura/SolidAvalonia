using Avalonia.Threading;

namespace SolidAvalonia;

/// <summary>
/// Effect computation that runs side effects
/// </summary>
internal class Effect : Computation
{
    private readonly Action _effect;

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

            // Track this computation
            Context.Push(this);

            // Push this effect as current owner for cleanup registration
            ReactiveSystem.Instance.Context.Push(this);

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
                Context.Pop();

                // Pop this effect as current owner
                ReactiveSystem.Instance.Context.Pop<IReactiveOwner>();
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
}