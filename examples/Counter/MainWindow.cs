using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Counter;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "SolidAvalonia Example";
        Width = 400;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Set the CounterView as the main content
        Content = new CounterView();
    }
}