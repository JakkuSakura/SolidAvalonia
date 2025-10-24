using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Counter.Advanced;
using Counter.CoreConcepts;
using Counter.UIPatterns;
using SolidAvalonia;

namespace Counter;

/// <summary>
/// A catalog view that organizes all SolidAvalonia examples in a structured way.
/// </summary>
public class ExamplesCatalog : Component
{
    protected override object Build() => CreateExampleTabs();

    private TabControl CreateExampleTabs()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Core Concepts Tab - Fundamentals of reactive programming
        var coreConcepts = new TabItem
        {
            Header = "Core Concepts",
            Content = CreateCoreConceptsTabs(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(coreConcepts);

        // UI Patterns Tab - Common UI patterns and techniques
        var uiPatterns = new TabItem
        {
            Header = "UI Patterns",
            Content = CreateUIPatternsTab(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(uiPatterns);

        // Advanced Tab - More complex techniques and optimizations
        var advanced = new TabItem
        {
            Header = "Advanced",
            Content = CreateAdvancedTab(),
            Padding = new Thickness(10)
        };
        tabControl.Items.Add(advanced);

        return tabControl;
    }

    private TabControl CreateCoreConceptsTabs()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Signal Examples
        var signalTab = new TabControl
        {
            TabStripPlacement = Dock.Bottom,
            Margin = new Thickness(5)
        };
        
        signalTab.Items.Add(new TabItem
        {
            Header = "Basic Signals",
            Content = SignalExample.SimpleCounter(),
            Padding = new Thickness(5)
        });
        
        signalTab.Items.Add(new TabItem
        {
            Header = "Signal Binding",
            Content = SignalExample.SignalBindingExample(),
            Padding = new Thickness(5)
        });
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Signals",
            Content = signalTab,
            Padding = new Thickness(5)
        });

        // Memo Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Memos",
            Content = MemoExample.DerivedValues(),
            Padding = new Thickness(10)
        });

        // Effect Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Effects",
            Content = EffectExample.SideEffects(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }

    private TabControl CreateUIPatternsTab()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Conditional Rendering Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Conditional Rendering",
            Content = ConditionalRenderingExample.ConditionalRendering(),
            Padding = new Thickness(10)
        });

        // Component Composition Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Component Composition",
            Content = CompositionExample.ComponentComposition(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }

    private TabControl CreateAdvancedTab()
    {
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        // Class-Based Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Class-based Component",
            Content = new ClassBasedCounter(),
            Padding = new Thickness(10)
        });

        // Cleanup Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Lifecycle & Cleanup",
            Content = CleanupExample.LifecycleManagement(),
            Padding = new Thickness(10)
        });

        // Throttling Example
        tabControl.Items.Add(new TabItem
        {
            Header = "Throttling & Debouncing",
            Content = ThrottlingExample.PerformanceOptimizations(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }
}
