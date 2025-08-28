using Avalonia.Controls;
using Avalonia;
using SolidAvalonia;


namespace Counter;

/// <summary>
/// A view that demonstrates functional components using SolidAvalonia
/// </summary>
public class FunctionalDemoView : Component
{
    protected override object Build()
    {
        // Use a tab control to show different examples
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };

        tabControl.Items.Add(new TabItem
        {
            Header = "Counter Example",
            Content = FunctionalCounterExample.CreateCounterDisplay(),
            Padding = new Thickness(10)
        });

        tabControl.Items.Add(new TabItem
        {
            Header = "Simple Counter",
            Content = FunctionalCounterExample.CreateCounter(),
            Padding = new Thickness(10)
        });

        tabControl.Items.Add(new TabItem
        {
            Header = "Conditional Rendering",
            Content = FunctionalCounterExample.CreateShowExample(),
            Padding = new Thickness(10)
        });

        tabControl.Items.Add(new TabItem
        {
            Header = "List Rendering",
            Content = FunctionalCounterExample.CreateListExample(),
            Padding = new Thickness(10)
        });

        return tabControl;
    }
}