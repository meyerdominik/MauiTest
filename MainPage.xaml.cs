using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace MauiTest;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        EnvironmentLabel.Text = $"CommunityToolkit.Maui 17.0.0 | Microsoft.Maui.Controls 11.0.10\n"
            + $".NET {Environment.Version} | {DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        OpenButton.IsEnabled = false;
        ModePicker.IsEnabled = false;
        var mode = (PopupOptionsMode)ModePicker.SelectedIndex;
        ReportLabel.Text = $"{EnvironmentLabel.Text}\nMode: {ModePicker.SelectedItem}\n";
        var first = new OwnershipPopup("Popup A", canOpenNested: true);
        first.Opened += (_, _) =>
        {
            RecordOwnership("A opened", first);
            first.OpenNestedButton.IsEnabled = true;
        };
        first.OpenNestedButton.Clicked += async (_, _) =>
        {
            first.OpenNestedButton.IsEnabled = false;
            try
            {
                var second = new OwnershipPopup("Popup B", canOpenNested: false);
                second.Opened += (_, _) => RecordOwnership("B opened while A remains open", first, second);
                await PresentAsync(second, mode);
                RecordOwnership("B closed; A remains open", first);
            }
            catch (Exception exception)
            {
                first.Status.Text = $"{exception.GetType().Name}: {exception.Message}";
                AppendReport(first.Status.Text);
            }
            finally
            {
                first.OpenNestedButton.IsEnabled = true;
            }
        };

        try
        {
            await PresentAsync(first, mode);
            AppendReport("A closed. Choose another mode to compare.");
        }
        catch (Exception exception)
        {
            AppendReport($"{exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            OpenButton.IsEnabled = true;
            ModePicker.IsEnabled = true;
        }
    }

    private Task<IPopupResult> PresentAsync(Popup popup, PopupOptionsMode mode) =>
        mode == PopupOptionsMode.Omitted
            ? this.ShowPopupAsync(popup)
            : this.ShowPopupAsync(popup, PopupAppearance.CreateOptions(mode));

    private void RecordOwnership(string stage, OwnershipPopup first, OwnershipPopup? second = null)
    {
        // Popup.Parent is Toolkit's public host Border. No private APIs or reflection.
        if (first.Parent is not Border a || a.StrokeShape is not Element aShape || a.Shadow is not { } aShadow)
            throw new InvalidOperationException("Expected a Toolkit host Border with a shape and shadow before Opened.");

        var report = $"{stage}\n"
            + $"A shape parent is A: {ReferenceEquals(aShape.Parent, a)}\n"
            + $"A shadow parent is A: {ReferenceEquals(aShadow.Parent, a)}";
        if (second is not null)
        {
            if (second.Parent is not Border b || b.StrokeShape is not Element bShape || b.Shadow is not { } bShadow)
                throw new InvalidOperationException("Expected the nested popup to have a host Border with a shape and shadow.");
            report += $"\nShapes shared: {ReferenceEquals(a.StrokeShape, b.StrokeShape)}"
                + $"\nShadows shared: {ReferenceEquals(a.Shadow, b.Shadow)}"
                + $"\nB shape parent is B: {ReferenceEquals(bShape.Parent, b)}"
                + $"\nB shadow parent is B: {ReferenceEquals(bShadow.Parent, b)}";
            second.Status.Text = report;
        }

        first.Status.Text = report;
        AppendReport(report);
    }

    private void AppendReport(string text)
    {
        ReportLabel.Text += $"\n{text}\n";
        Debug.WriteLine(text);
    }

    private async void OnCopyClicked(object? sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(ReportLabel.Text);
        }
        catch (Exception exception)
        {
            AppendReport($"Copy failed: {exception.GetType().Name}: {exception.Message}");
        }
    }
}
