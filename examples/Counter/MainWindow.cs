using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Counter;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "SolidAvalonia Examples";
        Width = 800;
        Height = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Create a tab control to display all examples
        var tabControl = new TabControl
        {
            TabStripPlacement = Dock.Top,
            Margin = new Thickness(5)
        };
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Class-based Counter",
            Content = new CounterView(),
            Padding = new Thickness(10)
        });
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Functional Components",
            Content = new FunctionalDemoView(),
            Padding = new Thickness(10)
        });
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Direct Functional Component",
            Content = FunctionalCounterExample.CreateCounterDisplay(),
            Padding = new Thickness(10)
        });
        
        // Add Signal Composition Example tabs
        tabControl.Items.Add(new TabItem
        {
            Header = "Signal Functions",
            Content = SignalCompositionExample.SignalFunctionExample(),
            Padding = new Thickness(10)
        });
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Shared Signals",
            Content = SignalCompositionExample.ComposedComponentsExample(),
            Padding = new Thickness(10)
        });
        
        tabControl.Items.Add(new TabItem
        {
            Header = "Theme Toggle",
            Content = SignalCompositionExample.ThemeToggleExample(),
            Padding = new Thickness(10)
        });
        
        Content = tabControl;
    }
}