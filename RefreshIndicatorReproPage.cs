using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace MauiTest;

public sealed partial class RefreshIndicatorReproPage : ContentPage
{
    private readonly RefreshReproViewModel _viewModel = new();

    public RefreshIndicatorReproPage(bool usePageTemplate = true, bool useAppSafeArea = true)
    {
        Title = "Home";
        BindingContext = _viewModel;
        this.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#F3F6F8"), Color.FromArgb("#0A0A0A"));

        var floatingTabs = useAppSafeArea && OperatingSystem.IsIOSVersionAtLeast(26);
        var containerEdges = floatingTabs
            ? new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.Container,
                SafeAreaRegions.Container, SafeAreaRegions.SoftInput)
            : new SafeAreaEdges(SafeAreaRegions.Container);

        if (usePageTemplate)
        {
            ControlTemplate = new ControlTemplate(() =>
            {
                var grid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new() { Height = GridLength.Auto },
                        new() { Height = GridLength.Star }
                    },
                    SafeAreaEdges = containerEdges
                };
                // Placeholder for AuthenticationPendingBanner in its normal,
                // signed-in state. Retains the template's Auto,* layout.
                grid.Add(new ContentView { IsVisible = false });
                grid.Add(new ContentPresenter(), 0, 1);
                return grid;
            });
        }

        var body = new VerticalStackLayout
        {
            Padding = new Thickness(16, 12, 16, 28),
            Spacing = 12,
            MaximumWidthRequest = 760,
            HorizontalOptions = LayoutOptions.Fill,
            SafeAreaEdges = floatingTabs ? SafeAreaEdges.None : new(SafeAreaRegions.Container)
        };

        var greeting = new VerticalStackLayout { Spacing = 4 };
        greeting.Add(new Label { Text = "Wednesday, 14 October", FontSize = 13, TextColor = Color.FromArgb("#C6E4F0") });
        greeting.Add(new Label { Text = "Good morning, Test User", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        greeting.Add(new Label { Text = "Your working day at a glance.", FontSize = 15, TextColor = Color.FromArgb("#DCECF3") });
        body.Add(new Border
        {
            Padding = new Thickness(16, 12),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Background = new LinearGradientBrush
            {
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new() { Color = Color.FromArgb("#006B91"), Offset = 0 },
                    new() { Color = Color.FromArgb("#173E52"), Offset = 1 }
                }
            },
            Content = greeting
        });

        var status = new Label { FontSize = 14 };
        status.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);
        status.SetBinding(Label.TextProperty, Binding.Create(static (RefreshReproViewModel vm) => vm.Status));
        body.Add(status);

        var sections = new VerticalStackLayout { Spacing = 14 };
        foreach (var title in new[] { "Header1", "Header2", "Header3", "Header4", "Header5", "Header6" })
        {
            var header = new Label
            {
                Text = title + "   ▾",
                Padding = new Thickness(16, 18),
                FontSize = 17,
                FontAttributes = FontAttributes.Bold
            };
            header.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#212121"), Colors.White);
            var rows = new VerticalStackLayout { Spacing = 0 };
            for (var row = 1; row <= 5; row++)
            {
                var label = new Label { Text = $"{title} — placeholder {row}", Padding = new Thickness(20, 18) };
                label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#212121"), Colors.White);
                rows.Add(label);
            }
            var card = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                Content = new Expander { Header = header, Content = rows, IsExpanded = false }
            };
            card.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#212121"));
            card.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E1E1E1"), Color.FromArgb("#404040"));
            sections.Add(card);
        }
        body.Add(sections);

        var scroll = new ScrollView { Content = body };
        if (floatingTabs)
            scroll.SafeAreaEdges = SafeAreaEdges.None;

        var refresh = new RefreshView { Content = scroll };
        refresh.SetAppThemeColor(RefreshView.RefreshColorProperty, Color.FromArgb("#006B91"), Color.FromArgb("#72D5FF"));
        refresh.SetBinding(RefreshView.CommandProperty, Binding.Create(static (RefreshReproViewModel vm) => vm.RefreshCommand));
        refresh.SetBinding(RefreshView.IsRefreshingProperty,
            Binding.Create(static (RefreshReproViewModel vm) => vm.IsRefreshing,
                mode: BindingMode.TwoWay));
        Content = refresh;
    }

    protected override void OnDisappearing()
    {
        _viewModel.CancelRefresh();
        base.OnDisappearing();
    }
}

public sealed class RefreshReproViewModel : BindableObject
{
    private CancellationTokenSource? _refreshCancellation;
    private bool _isRefreshing;
    private string _status = "Pull down at the top. Refresh stays active for 15 seconds.";

    public RefreshReproViewModel() => RefreshCommand = new Command(async () => await RefreshAsync());

    public Command RefreshCommand { get; }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing == value) return;
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    private async Task RefreshAsync()
    {
        if (_refreshCancellation is not null) return;
        using var cancellation = new CancellationTokenSource();
        _refreshCancellation = cancellation;
        IsRefreshing = true;
        try
        {
            for (var remaining = 15; remaining > 0; remaining--)
            {
                Status = $"Refreshing… {remaining} seconds remaining. Observe the native spinner.";
                await Task.Delay(TimeSpan.FromSeconds(1), cancellation.Token);
            }
            Status = "Refresh completed. Pull again with sections expanded.";
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            Status = "Refresh cancelled because the page was left.";
        }
        finally
        {
            IsRefreshing = false;
            _refreshCancellation = null;
        }
    }

    public void CancelRefresh() => _refreshCancellation?.Cancel();
}

public sealed partial class RefreshReproShell : Shell
{
    public RefreshReproShell(bool usePageTemplate = true, bool useAppSafeArea = true)
    {
        BackgroundColor = Color.FromArgb("#006B91");
        Shell.SetTitleColor(this, Colors.White);
        Shell.SetForegroundColor(this, Colors.White);
        var tabs = new TabBar();
        var home = new Tab { Title = "Page1" };
        home.Items.Add(new ShellContent
        {
            Route = "RefreshRepro",
            ContentTemplate = new DataTemplate(() => new RefreshIndicatorReproPage(usePageTemplate, useAppSafeArea))
        });
        tabs.Items.Add(home);
        foreach (var title in new[] { "Page2", "Page3" })
        {
            var tab = new Tab { Title = title };
            tab.Items.Add(new ShellContent
            {
                Content = new ContentPage { Title = title, Content = new Label { Text = "Return to Page1 to test refresh." } }
            });
            tabs.Items.Add(tab);
        }
        Items.Add(tabs);
    }
}
