using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Counter.ClassBased;
using Counter.Functional;
using Counter.Composition;

namespace Counter;

/// <summary>
/// A catalog view that organizes all SolidAvalonia examples in a structured way.
/// </summary>
public class ExamplesCatalog : ContentControl
{
    public ExamplesCatalog()
    {
        this.Content = CreateExampleTabs();
    }

    private TabControl CreateExampleTabs()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Class-based Examples
        var classBasedTab = new TabItem
        {
            Header = "Class-based",
            Content = new ClassBasedCounter(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(classBasedTab);

        // Functional Examples
        var functionalTab = new TabItem
        {
            Header = "Functional",
            Content = CreateFunctionalExamplesTabs(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(functionalTab);

        // Composition Examples
        var compositionTab = new TabItem
        {
            Header = "Composition",
            Content = CreateCompositionExamplesTabs(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(compositionTab);

        return tabControl;
    }

    private TabControl CreateFunctionalExamplesTabs()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Advanced Counter Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Advanced Counter",
            Content = CounterExamples.AdvancedCounter(),
            Padding = new Thickness(10)
        });

        // Simple Counter Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Simple Counter",
            Content = CounterExamples.SimpleCounter(),
            Padding = new Thickness(10)
        });

        // Conditional Rendering Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Conditional Rendering",
            Content = CounterExamples.ConditionalRenderingExample(),
            Padding = new Thickness(10)
        });

        // Dynamic List Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Dynamic List",
            Content = CounterExamples.DynamicListExample(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }

    private TabControl CreateCompositionExamplesTabs()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Signal Functions Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Signal Functions",
            Content = SignalCompositionExamples.SignalFunctionExample(),
            Padding = new Thickness(10)
        });

        // Shared Signals Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Shared Signals",
            Content = SignalCompositionExamples.SharedSignalsExample(),
            Padding = new Thickness(10)
        });

        // Theme Toggle Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Theme Toggle",
            Content = SignalCompositionExamples.ThemeToggleExample(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }
}