using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using System.Reactive.Disposables;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.ReactiveUI;

namespace SolidAvalonia;

/// <summary>
/// Base class for creating reactive Avalonia controls with SolidJS-like API
/// </summary>
public class SolidControl : UserControl, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly DependencyTracker _dependencyTracker = new();
    private bool _disposed;

    #region Dependency Tracking System

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

    #endregion

    #region Signal System Public API

    /// <summary>
    /// Creates a reactive signal with getter and setter
    /// </summary>
    protected (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        var signal = new Signal<T>(initialValue, _dependencyTracker);
        _disposables.Add(signal);

        return (signal.Get, signal.Set);
    }

    /// <summary>
    /// Creates a computed value that automatically updates when dependencies change
    /// </summary>
    protected Func<T> CreateMemo<T>(Func<T> computation)
    {
        var memo = new Memo<T>(computation, _dependencyTracker);
        _disposables.Add(memo);

        return memo.Get;
    }

    /// <summary>
    /// Creates an effect that runs when dependencies change
    /// </summary>
    protected void CreateEffect(Action effect)
    {
        var effectWrapper = new Effect(effect, _dependencyTracker);
        _disposables.Add(effectWrapper);

        // Run immediately
        effectWrapper.Run();
    }

    /// <summary>
    /// Subscribe to an observable
    /// </summary>
    protected void Subscribe<T>(IObservable<T> observable, Action<T> onNext)
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

    #endregion

    #region Layout Helpers

    /// <summary>
    /// Creates a vertical layout container with consistent spacing and padding
    /// </summary>
    protected Panel VStack(double spacing = 10, double margin = 20, params Control[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = spacing,
            Margin = new Thickness(margin)
        };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>
    /// Creates a horizontal layout container with consistent spacing and padding
    /// </summary>
    protected Panel HStack(double spacing = 10, double margin = 0, params Control[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = spacing,
            Margin = new Thickness(margin)
        };

        foreach (var child in children)
        {
            panel.Children.Add(child);
        }

        return panel;
    }

    /// <summary>
    /// Creates a card-like container with border, background, and padding
    /// </summary>
    protected Border Card(Control content, IBrush? background = null, double cornerRadius = 8, double padding = 15,
        double margin = 5)
    {
        return new Border
        {
            Background = background ?? new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(cornerRadius),
            Padding = new Thickness(padding),
            Margin = new Thickness(margin),
            BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
            BorderThickness = new Thickness(1),
            Child = content
        };
    }

    /// <summary>
    /// Creates a section with a header and content
    /// </summary>
    protected Panel Section(string title, Control content, double fontSize = 16, double spacing = 10)
    {
        var header = new TextBlock
        {
            Text = title,
            FontSize = fontSize,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 5)
        };

        return VStack(spacing, 0, header, content);
    }

    /// <summary>
    /// Creates a responsive grid layout
    /// </summary>
    protected Grid CreateGrid(string columnDefinitions = "*", string rowDefinitions = "*")
    {
        var grid = new Grid();

        // Parse column definitions
        var cols = columnDefinitions.Split(',');
        foreach (var col in cols)
        {
            var trimmed = col.Trim();
            if (trimmed == "*")
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            else if (trimmed.EndsWith("*"))
            {
                if (double.TryParse(trimmed.TrimEnd('*'), out var factor))
                    grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (trimmed.Equals("auto", StringComparison.OrdinalIgnoreCase))
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            else if (double.TryParse(trimmed, out var width))
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(width)));
        }

        // Parse row definitions
        var rows = rowDefinitions.Split(',');
        foreach (var row in rows)
        {
            var trimmed = row.Trim();
            if (trimmed == "*")
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            else if (trimmed.EndsWith("*"))
            {
                if (double.TryParse(trimmed.TrimEnd('*'), out var factor))
                    grid.RowDefinitions.Add(new RowDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (trimmed.Equals("auto", StringComparison.OrdinalIgnoreCase))
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            else if (double.TryParse(trimmed, out var height))
                grid.RowDefinitions.Add(new RowDefinition(new GridLength(height)));
        }

        return grid;
    }

    /// <summary>
    /// Helper to add a control to a grid at specific position
    /// </summary>
    protected T GridChild<T>(T control, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
        where T : Control
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, column);
        Grid.SetRowSpan(control, rowSpan);
        Grid.SetColumnSpan(control, columnSpan);
        return control;
    }

    /// <summary>
    /// Creates a button with consistent styling
    /// </summary>
    protected Button StyledButton(string content, Action? onClick = null, double width = double.NaN, double height = 35,
        IBrush? background = null, IBrush? foreground = null)
    {
        var button = new Button
        {
            Content = content,
            Height = height,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 8),
            CornerRadius = new CornerRadius(6)
        };

        if (!double.IsNaN(width))
            button.Width = width;

        if (background != null)
            button.Background = background;

        if (foreground != null)
            button.Foreground = foreground;

        if (onClick != null)
            button.Click += (_, _) => onClick();

        return button;
    }

    /// <summary>
    /// Creates a text input with consistent styling
    /// </summary>
    protected TextBox StyledTextBox(string watermark = "", double width = 200, double height = 35)
    {
        return new TextBox
        {
            Watermark = watermark,
            Width = width,
            Height = height,
            Padding = new Thickness(10, 8),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(206, 212, 218))
        };
    }

    /// <summary>
    /// Creates a styled text block
    /// </summary>
    protected TextBlock StyledText(string text = "", double fontSize = 14, FontWeight fontWeight = FontWeight.Normal,
        IBrush? foreground = null, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = fontWeight,
            HorizontalAlignment = alignment
        };

        if (foreground != null)
            textBlock.Foreground = foreground;

        return textBlock;
    }

    /// <summary>
    /// Creates a separator line
    /// </summary>
    protected Separator CreateSeparator(double margin = 10)
    {
        return new Separator
        {
            Margin = new Thickness(0, margin)
        };
    }

    /// <summary>
    /// Creates a centered container
    /// </summary>
    protected Panel Centered(Control content, double maxWidth = double.NaN)
    {
        var container = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (!double.IsNaN(maxWidth))
            container.MaxWidth = maxWidth;

        container.Children.Add(content);
        return container;
    }

    #endregion

    #region Lifecycle

    protected virtual void HandleError(string errorMessage)
    {
        System.Diagnostics.Debug.WriteLine($"[SolidControl Error] {errorMessage}");
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _disposables.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}

