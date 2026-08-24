using System.Reflection;

namespace MauiTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            mauiversion.Text = GetMauiVersion();
            dotnetversion.Text = Environment.Version.ToString();
        }

        private static string GetMauiVersion() => typeof(Element)
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? typeof(Element).Assembly.GetName().Version?.ToString()
                ?? "N/A";
    }
}
