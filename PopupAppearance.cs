using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Shapes;

namespace MauiTest;

internal enum PopupOptionsMode
{
    SeparateDefaults,
    Empty,
    Omitted,
    IndependentAppearance
}

internal static class PopupAppearance
{
    internal static IPopupOptions CreateOptions(PopupOptionsMode mode) => mode switch
    {
        PopupOptionsMode.SeparateDefaults => new PopupOptions(),
        PopupOptionsMode.Empty => PopupOptions.Empty,
        PopupOptionsMode.IndependentAppearance => new PopupOptions
        {
            Shape = new RoundRectangle { CornerRadius = 20, Stroke = Colors.LightGray, StrokeThickness = 2 },
            Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(20, 20), Radius = 40, Opacity = 0.8f }
        },
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
