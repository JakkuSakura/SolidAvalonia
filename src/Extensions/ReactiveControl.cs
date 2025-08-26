using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia.Extensions;

public class ReactiveControl<T>(Func<T> getter) : ViewBase
    where T : Control
{
    private bool _isInitialized;
    protected override object Build() => getter();

    public void Register(IReactiveSystem rc)
    {
        rc.CreateEffect(() =>
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }
            else
                Reload();
        });
    }
}