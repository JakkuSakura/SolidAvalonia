using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Counter;

/// <summary>
/// Main window for the SolidAvalonia examples application.
/// Presents a catalog of examples demonstrating various patterns and techniques.
/// </summary>
public class MainWindow : Window
{
    public MainWindow()
    {
        // Configure window properties
        Title = "SolidAvalonia Examples";
        Width = 900;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Create a header with title and description
        var header = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        header.Children.Add(new TextBlock
        {
            Text = "SolidAvalonia Examples",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        
        header.Children.Add(new TextBlock
        {
            Text = "Explore different approaches to building reactive UI components",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
        });
        
        // Create the examples catalog that contains all example components
        var examplesCatalog = new ExamplesCatalog();
        
        // Combine header and content in a main layout panel
        var mainPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Children = { header, examplesCatalog }
        };
        
        // Set the content of the window
        Content = mainPanel;
    }
}