namespace MauiTest
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            Dictionary<string, object?> param = new(1) {
                { nameof(DetailsPage.ID), (long?)1 }
            };
            await Shell.Current.GoToAsync(nameof(DetailsPage), param);
        }
    }
}
