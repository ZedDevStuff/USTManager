using System;
using System.Collections.Generic;
using System.Text;

namespace USTManager.Utility
{
public class ListenableProperty<T>
{
    private T _Value;
    public T Value
    {
        get => _Value;
        set
        {
            if (value.Equals(_Value)) return;
            _Value = value;
            OnValueChanged?.Invoke(value);
        }
    }

    private ListenableProperty(T value) => _Value = value;

    public event Action<T> OnValueChanged;
        
    public static implicit operator ListenableProperty<T>(T value) => new ListenableProperty<T>(value);
    public static implicit operator T(ListenableProperty<T> value) => value.Value;
}
}
