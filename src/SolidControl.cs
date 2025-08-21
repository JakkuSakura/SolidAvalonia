using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls;
using System.Reactive.Disposables;
using Avalonia.Threading;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia;
using Avalonia.ReactiveUI;

namespace SolidAvalonia;

public class SolidControl : UserControl, IDisposable
{
    // Signal storage with proper disposal tracking
    private readonly Dictionary<string, IDisposable> _signals = new();
    private readonly CompositeDisposable _effects = new();
    private readonly CompositeDisposable _subscriptions = new();
    private bool _disposed;

    #region Signal Methods (unchanged)
    protected (Func<T>, Action<T>) CreateSignal<T>(T initialValue)
    {
        var subject = new BehaviorSubject<T>(initialValue);
        var key = Guid.NewGuid().ToString();
        _signals[key] = subject;

        return (() => subject.Value, value => 
        {
            if (!_disposed)
                subject.OnNext(value);
        });
    }

    protected Func<T?> CreateMemo<T>(Func<T> computation)
    {
        var memoSubject = new BehaviorSubject<T?>(default(T));
        var key = Guid.NewGuid().ToString();
        _signals[key] = memoSubject;

        try
        {
            var initialValue = computation();
            memoSubject.OnNext(initialValue);
        }
        catch (Exception ex)
        {
            HandleError($"Memo computation error: {ex.Message}");
        }

        var subscription = Observable
            .Interval(TimeSpan.FromMilliseconds(100))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ =>
            {
                if (_disposed) return;
                
                try
                {
                    var newValue = computation();
                    if (!EqualityComparer<T>.Default.Equals(memoSubject.Value, newValue))
                    {
                        memoSubject.OnNext(newValue);
                    }
                }
                catch (Exception ex)
                {
                    HandleError($"Memo computation error: {ex.Message}");
                }
            });

        _effects.Add(subscription);
        return () => _disposed ? default(T) : memoSubject.Value;
    }

    protected void CreateEffect(Action effect)
    {
        var subscription = Observable
            .Interval(TimeSpan.FromMilliseconds(50))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ =>
            {
                if (_disposed) return;
                
                try
                {
                    effect();
                }
                catch (Exception ex)
                {
                    HandleError($"Effect error: {ex.Message}");
                }
            });
            
        _effects.Add(subscription);
    }
    
    protected void Subscribe<T>(IObservable<T> observable, Action<T> onNext)
    {
        var subscription = observable.Subscribe(onNext);
        _subscriptions.Add(subscription);
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
    protected Border Card(Control content, IBrush? background = null, double cornerRadius = 8, double padding = 15, double margin = 5)
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
            if (col.Trim() == "*")
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            else if (col.Trim().EndsWith("*"))
            {
                var factor = double.Parse(col.Trim().TrimEnd('*'));
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (col.Trim().ToLower() == "auto")
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            else
            {
                var width = double.Parse(col.Trim());
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(width)));
            }
        }

        // Parse row definitions
        var rows = rowDefinitions.Split(',');
        foreach (var row in rows)
        {
            if (row.Trim() == "*")
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            else if (row.Trim().EndsWith("*"))
            {
                var factor = double.Parse(row.Trim().TrimEnd('*'));
                grid.RowDefinitions.Add(new RowDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (row.Trim().ToLower() == "auto")
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            else
            {
                var height = double.Parse(row.Trim());
                grid.RowDefinitions.Add(new RowDefinition(new GridLength(height)));
            }
        }

        return grid;
    }

    /// <summary>
    /// Helper to add a control to a grid at specific position
    /// </summary>
    protected T GridChild<T>(T control, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1) where T : Control
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
    protected Button StyledButton(string content, double width = double.NaN, double height = 35, 
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

    protected virtual void HandleError(string errorMessage)
    {
        Console.WriteLine(errorMessage);
    }

    protected int ActiveSignalCount => _signals.Count;
    protected int ActiveEffectCount => _effects.Count;

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        
        foreach (var signal in _signals.Values)
        {
            signal?.Dispose();
        }
        _signals.Clear();
        
        _effects?.Dispose();
        _subscriptions?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}