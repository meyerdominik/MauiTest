using CommunityToolkit.Maui;

namespace MauiTest
{
    public partial class MainPage : ContentPage
    {
        private readonly IPopupService _popupService;

        public MainPage(IPopupService popupService)
        {
            _popupService = popupService;

            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            _popupService.ShowPopup<PopupContentView>(this);
        }
    }
}
