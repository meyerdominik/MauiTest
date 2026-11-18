using CommunityToolkit.Maui.Views;

namespace MauiTest;

internal sealed class OwnershipPopup : Popup
{
    internal Label Status { get; } = new() { Text = "Waiting for Opened...", FontSize = 14 };
    internal Button OpenNestedButton { get; } = new()
    {
        Text = "Open popup B",
        AutomationId = "OpenPopupB",
        MinimumHeightRequest = 48,
        IsEnabled = false
    };

    internal OwnershipPopup(string title, bool canOpenNested)
    {
        WidthRequest = 320;
        HeightRequest = 460;
        // Toolkit clamps the requested size to the allocated safe viewport.
        var close = new Button { Text = $"Close {title}", AutomationId = canOpenNested ? "ClosePopupA" : "ClosePopupB", MinimumHeightRequest = 48 };
        close.Clicked += async (_, _) =>
        {
            close.IsEnabled = false;
            try
            {
                await CloseAsync();
            }
            catch (Exception exception)
            {
                Status.Text = $"{exception.GetType().Name}: {exception.Message}";
                close.IsEnabled = true;
            }
        };

        OpenNestedButton.IsVisible = canOpenNested;
        var heading = new Label { Text = title, FontSize = 22 };
        SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level1);
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children = { heading, Status, OpenNestedButton, close }
            }
        };
    }
}
