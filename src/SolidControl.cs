using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

/// <summary>
/// Base class for creating reactive Avalonia controls with SolidJS-like API
/// </summary>
public class SolidControl : UserControl, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly IReactiveSystem _reactiveSystem;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SolidControl class
    /// </summary>
    public SolidControl()
    {
        _reactiveSystem = new SolidReactiveSystem();
        _disposables.Add(_reactiveSystem);
    }

    /// <summary>
    /// Initializes a new instance of the SolidControl class with custom dependencies
    /// </summary>
    public SolidControl(IReactiveSystem reactiveSystem)
    {
        _reactiveSystem = reactiveSystem;
        _disposables.Add(_reactiveSystem);
    }

    #region Reactive System API

    /// <summary>
    /// Creates a reactive signal with getter and setter
    /// </summary>
    protected (Func<T>, Action<T>) CreateSignal<T>(T initialValue) => 
        _reactiveSystem.CreateSignal(initialValue);

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    protected Func<T> CreateMemo<T>(Func<T> computation) => 
        _reactiveSystem.CreateMemo(computation);

    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    protected void CreateEffect(Action effect) => 
        _reactiveSystem.CreateEffect(effect);

    /// <summary>
    /// Subscribe to an observable
    /// </summary>
    protected void Subscribe<T>(IObservable<T> observable, Action<T> onNext) => 
        _reactiveSystem.Subscribe(observable, onNext);

    #endregion

    #region Lifecycle

    /// <summary>
    /// Handles errors that occur in the control
    /// </summary>
    protected virtual void HandleError(string errorMessage)
    {
        _reactiveSystem.HandleError(errorMessage);
    }

    /// <summary>
    /// Called when the control is detached from the visual tree
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Disposes all resources used by the control
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