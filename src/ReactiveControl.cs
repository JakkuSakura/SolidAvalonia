using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using SolidAvalonia.ReactiveSystem;

namespace SolidAvalonia;

public class ReactiveControl<T> : ViewBase
    where T : Control
{
    private bool _isInitialized;
    private readonly Func<T> _getter;

    internal ReactiveControl(IReactiveSystem rs, Func<T> getter): base(true)
    {
        _getter = getter;
        Register(rs);
    }

    protected override object Build() => _getter();

    private void Register(IReactiveSystem rc)
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