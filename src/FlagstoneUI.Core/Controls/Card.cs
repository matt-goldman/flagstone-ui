using Microsoft.Maui.Controls;

namespace FlagstoneUI.Core.Controls;

public class Card : ContentView
{
    public static readonly BindableProperty ElevationProperty = BindableProperty.Create(
        nameof(Elevation), typeof(int), typeof(Card), 0);

    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }
}
