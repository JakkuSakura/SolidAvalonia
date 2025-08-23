using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.ReactiveUI;

namespace SolidAvalonia.ReactiveSystem;

/// <summary>
/// Implementation of the reactive system providing signal, memo, and effect functionality
/// </summary>
public class SolidReactiveSystem : IReactiveSystem
{
    private readonly CompositeDisposable _disposables = new();
    private readonly DependencyTracker _dependencyTracker = new();
    private bool _disposed;
    
    /// <summary>
    /// Tracks dependencies for reactive computations
    /// </summary>
    private class DependencyTracker
    {
        private readonly Stack<HashSet<IReactiveNode>> _trackingStack = new();

        public void StartTracking(HashSet<IReactiveNode> dependencies)
        {
            _trackingStack.Push(dependencies);
        }

        public void StopTracking()
        {
            if (_trackingStack.Count > 0)
                _trackingStack.Pop();
        }

        public void RegisterDependency(IReactiveNode node)
        {
            if (_trackingStack.Count > 0)
            {
                _trackingStack.Peek().Add(node);
            }
        }

        public bool IsTracking => _trackingStack.Count > 0;
    }

    /// <summary>
    /// Base interface for reactive nodes
    /// </summary>
    private interface IReactiveNode : IDisposable
    {
        IObservable<object?> Changed { get; }
    }

    /// <summary>
    /// Signal implementation with dependency tracking
    /// </summary>
    private class Signal<T> : IReactiveNode
    {
        private readonly BehaviorSubject<T> _subject;
        private readonly DependencyTracker _tracker;
        private readonly Subject<object?> _changed = new();

        public Signal(T initialValue, DependencyTracker tracker)
        {
            _subject = new BehaviorSubject<T>(initialValue);
            _tracker = tracker;

            _subject.Skip(1).Subscribe(_ => _changed.OnNext(null));
        }

        public T Get()
        {
            _tracker.RegisterDependency(this);
            return _subject.Value;
        }

        public void Set(T value)
        {
            if (!EqualityComparer<T>.Default.Equals(_subject.Value, value))
            {
                _subject.OnNext(value);
            }
        }

        public IObservable<object?> Changed => _changed;

        public void Dispose()
        {
            _subject?.Dispose();
            _changed?.Dispose();
        }
    }

    /// <summary>
    /// Memo implementation with automatic dependency tracking
    /// </summary>
    private class Memo<T> : IReactiveNode
    {
        private readonly Func<T> _computation;
        private readonly DependencyTracker _tracker;
        private readonly BehaviorSubject<T> _value;
        private readonly Subject<object?> _changed = new();
        private readonly CompositeDisposable _subscriptions = new();
        private HashSet<IReactiveNode> _dependencies = new();

        public Memo(Func<T> computation, DependencyTracker tracker)
        {
            _computation = computation;
            _tracker = tracker;

            // Initial computation
            var initialValue = ComputeWithTracking();
            _value = new BehaviorSubject<T>(initialValue);

            _value.Skip(1).Subscribe(_ => _changed.OnNext(null));
        }

        private T ComputeWithTracking()
        {
            // Clear old subscriptions
            _subscriptions.Clear();

            // Track dependencies
            var newDependencies = new HashSet<IReactiveNode>();
            _tracker.StartTracking(newDependencies);

            T result;
            try
            {
                result = _computation();
            }
            finally
            {
                _tracker.StopTracking();
            }

            // Subscribe to new dependencies
            foreach (var dep in newDependencies)
            {
                var subscription = dep.Changed.Subscribe(_ => Recompute());
                _subscriptions.Add(subscription);
            }

            _dependencies = newDependencies;
            return result;
        }

        private void Recompute()
        {
            var newValue = ComputeWithTracking();
            if (!EqualityComparer<T>.Default.Equals(_value.Value, newValue))
            {
                _value.OnNext(newValue);
            }
        }

        public T Get()
        {
            _tracker.RegisterDependency(this);
            return _value.Value;
        }

        public IObservable<object?> Changed => _changed;

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _value?.Dispose();
            _changed?.Dispose();
        }
    }

    /// <summary>
    /// Effect implementation with automatic dependency tracking
    /// </summary>
    private class Effect : IDisposable
    {
        private readonly Action _effect;
        private readonly DependencyTracker _tracker;
        private readonly CompositeDisposable _subscriptions = new();
        private HashSet<IReactiveNode> _dependencies = new();

        public Effect(Action effect, DependencyTracker tracker)
        {
            _effect = effect;
            _tracker = tracker;
        }

        public void Run()
        {
            // Clear old subscriptions
            _subscriptions.Clear();

            // Track dependencies
            var newDependencies = new HashSet<IReactiveNode>();
            _tracker.StartTracking(newDependencies);

            try
            {
                // Run on UI thread if needed
                if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                {
                    _effect();
                }
                else
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(_effect).Wait();
                }
            }
            finally
            {
                _tracker.StopTracking();
            }

            // Subscribe to new dependencies
            foreach (var dep in newDependencies)
            {
                var subscription = dep.Changed
                    .Throttle(TimeSpan.FromMilliseconds(16)) // 60 FPS max
                    .ObserveOn(AvaloniaScheduler.Instance)
                    .Subscribe(_ => Run());
                _subscriptions.Add(subscription);
            }

            _dependencies = newDependencies;
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }

    #region IReactiveSystem Implementation

    /// <summary>
    /// Creates a reactive signal with getter and setter
    /// </summary>
    public (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        var signal = new Signal<T>(initialValue, _dependencyTracker);
        _disposables.Add(signal);

        return (signal.Get, signal.Set);
    }

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    public Func<T> CreateMemo<T>(Func<T> computation)
    {
        var memo = new Memo<T>(computation, _dependencyTracker);
        _disposables.Add(memo);

        return memo.Get;
    }

    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    public void CreateEffect(Action effect)
    {
        var effectWrapper = new Effect(effect, _dependencyTracker);
        _disposables.Add(effectWrapper);

        // Run immediately
        effectWrapper.Run();
    }

    /// <summary>
    /// Subscribe to an observable
    /// </summary>
    public void Subscribe<T>(IObservable<T> observable, Action<T> onNext)
    {
        var subscription = observable
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(value =>
            {
                if (!_disposed)
                {
                    try
                    {
                        onNext(value);
                    }
                    catch (Exception ex)
                    {
                        HandleError($"Subscription error: {ex.Message}");
                    }
                }
            });

        _disposables.Add(subscription);
    }

    /// <summary>
    /// Handler for errors that occur in the reactive system
    /// </summary>
    public virtual void HandleError(string errorMessage)
    {
        System.Diagnostics.Debug.WriteLine($"[SolidReactiveSystem Error] {errorMessage}");
    }

    /// <summary>
    /// Disposes all resources used by the reactive system
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _disposables.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}