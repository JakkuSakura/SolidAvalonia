using Avalonia.Controls;

namespace SolidAvalonia;

/// <summary>
/// Base class for reactive components that provides signal-based reactivity.
/// </summary>
public class Reactive : Component
{
    private bool _isInitialized;

    public Reactive(Func<Control> func) : base(func)
    {
        Register();
    }

    // Create an effect to rebuild the component when dependencies change
    private void Register()
    {
        CreateEffect(() =>
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                OnCreatedCore();
                Initialize();
            }
            else
            {
                Reload();
            }
        });
    }
}