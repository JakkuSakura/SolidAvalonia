using Avalonia;
using Avalonia.Controls;

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