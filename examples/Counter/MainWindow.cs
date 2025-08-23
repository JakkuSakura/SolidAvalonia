using Avalonia;
using Avalonia.Controls;

namespace Counter;

public class MainWindow : Window
{
    public MainWindow()
    {
        Title = "SolidAvalonia Counter Example";
        Width = 400;
        Height = 300;
        
        // Set the CounterView as the main content
        Content = new CounterView();
    }
}