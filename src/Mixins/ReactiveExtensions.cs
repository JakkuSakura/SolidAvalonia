using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for reactive controls
/// </summary>
public static class ReactiveExtensions
{
    /// <summary>
    /// Creates a text block that automatically updates when its text source changes
    /// </summary>
    public static TextBlock ReactiveText(this SolidControl control, Func<string> textFactory, double fontSize = 14, 
        FontWeight fontWeight = FontWeight.Normal, IBrush? foreground = null, 
        TextAlignment textAlignment = TextAlignment.Left)
    {
        var textBlock = new TextBlock
        {
            FontSize = fontSize,
            FontWeight = fontWeight,
            TextAlignment = textAlignment
        };

        if (foreground != null)
            textBlock.Foreground = foreground;

        control.CreateEffect(() => textBlock.Text = textFactory());
        
        return textBlock;
    }

    /// <summary>
    /// Creates a button with a reactive content that updates automatically
    /// </summary>
    public static Button ReactiveButton(this SolidControl control, Func<string> contentFactory, 
        Action? onClick = null, double width = double.NaN, double height = 35,
        IBrush? background = null, IBrush? foreground = null)
    {
        var button = new Button
        {
            Height = height,
            Padding = new Avalonia.Thickness(20, 8),
            CornerRadius = new Avalonia.CornerRadius(6)
        };

        if (!double.IsNaN(width))
            button.Width = width;

        if (background != null)
            button.Background = background;

        if (foreground != null)
            button.Foreground = foreground;

        if (onClick != null)
            button.Click += (_, _) => onClick();

        control.CreateEffect(() => button.Content = contentFactory());
        
        return button;
    }

    /// <summary>
    /// Creates a text box with reactive binding to a signal
    /// </summary>
    public static TextBox ReactiveTextBox(this SolidControl control, Func<string> getValue, Action<string> setValue,
        string watermark = "", double width = 200, double height = 35)
    {
        var textBox = new TextBox
        {
            Watermark = watermark,
            Width = width,
            Height = height,
            Padding = new Avalonia.Thickness(10, 8),
            CornerRadius = new Avalonia.CornerRadius(6),
            BorderThickness = new Avalonia.Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(206, 212, 218))
        };

        // Initial value
        textBox.Text = getValue();

        // When signal changes, update TextBox
        control.CreateEffect(() => textBox.Text = getValue());

        // When TextBox changes, update signal
        textBox.TextChanged += (_, _) => setValue(textBox.Text ?? string.Empty);
        
        return textBox;
    }

    /// <summary>
    /// Creates a checkbox with reactive binding to a signal
    /// </summary>
    public static CheckBox ReactiveCheckBox(this SolidControl control, Func<bool> getValue, Action<bool> setValue,
        string content = "", double width = double.NaN, double height = double.NaN)
    {
        var checkBox = new CheckBox
        {
            Content = content
        };

        if (!double.IsNaN(width))
            checkBox.Width = width;

        if (!double.IsNaN(height))
            checkBox.Height = height;

        // Initial value
        checkBox.IsChecked = getValue();

        // When signal changes, update CheckBox
        control.CreateEffect(() => checkBox.IsChecked = getValue());

        // When CheckBox changes, update signal
        checkBox.IsCheckedChanged += (_, _) => setValue(checkBox.IsChecked ?? false);
        
        return checkBox;
    }

    /// <summary>
    /// Creates a reactive toggle button
    /// </summary>
    public static ToggleButton ReactiveToggle(this SolidControl control, Func<bool> getValue, Action<bool> setValue,
        string content = "", double width = double.NaN, double height = double.NaN)
    {
        var toggleButton = new ToggleButton
        {
            Content = content
        };

        if (!double.IsNaN(width))
            toggleButton.Width = width;

        if (!double.IsNaN(height))
            toggleButton.Height = height;

        // Initial value
        toggleButton.IsChecked = getValue();

        // When signal changes, update ToggleButton
        control.CreateEffect(() => toggleButton.IsChecked = getValue());

        // When ToggleButton changes, update signal
        toggleButton.IsCheckedChanged += (_, _) => setValue(toggleButton.IsChecked ?? false);
        
        return toggleButton;
    }

    /// <summary>
    /// Creates a reactive slider
    /// </summary>
    public static Slider ReactiveSlider(this SolidControl control, Func<double> getValue, Action<double> setValue,
        double minimum = 0, double maximum = 100, double width = double.NaN, double height = double.NaN)
    {
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum
        };

        if (!double.IsNaN(width))
            slider.Width = width;

        if (!double.IsNaN(height))
            slider.Height = height;

        // Initial value
        slider.Value = getValue();

        // When signal changes, update Slider
        control.CreateEffect(() => slider.Value = getValue());

        // When Slider changes, update signal
        slider.ValueChanged += (_, e) => setValue(e.NewValue);
        
        return slider;
    }

    /// <summary>
    /// Creates a reactive combo box
    /// </summary>
    public static ComboBox ReactiveComboBox<T>(this SolidControl control, Func<T> getValue, Action<T> setValue,
        T[] items, double width = double.NaN, double height = double.NaN)
    {
        var comboBox = new ComboBox
        {
            Width = width,
            Height = height
        };

        // Set items
        foreach (var item in items)
        {
            comboBox.Items.Add(item);
        }

        // Initial value
        comboBox.SelectedItem = getValue();

        // When signal changes, update ComboBox
        control.CreateEffect(() => comboBox.SelectedItem = getValue());

        // When ComboBox changes, update signal
        comboBox.SelectionChanged += (_, _) => 
        {
            if (comboBox.SelectedItem is T value)
            {
                setValue(value);
            }
        };
        
        return comboBox;
    }

    /// <summary>
    /// Creates a reactive progress bar
    /// </summary>
    public static ProgressBar ReactiveProgressBar(this SolidControl control, Func<double> getValue,
        double minimum = 0, double maximum = 100, double width = double.NaN, double height = double.NaN)
    {
        var progressBar = new ProgressBar
        {
            Minimum = minimum,
            Maximum = maximum
        };

        if (!double.IsNaN(width))
            progressBar.Width = width;

        if (!double.IsNaN(height))
            progressBar.Height = height;

        // Set up the reactive binding
        control.CreateEffect(() => progressBar.Value = getValue());
        
        return progressBar;
    }

    /// <summary>
    /// Creates a control that is conditionally visible based on a condition
    /// </summary>
    public static T ShowWhen<T>(this T control, SolidControl parent, Func<bool> condition) where T : Control
    {
        parent.CreateEffect(() => control.IsVisible = condition());
        return control;
    }
}