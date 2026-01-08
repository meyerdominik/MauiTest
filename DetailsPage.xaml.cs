namespace MauiTest;


[QueryProperty(nameof(ID), nameof(ID))]
public partial class DetailsPage : ContentPage
{
	public long? ID { get; set; }

	public DetailsPage()
	{
		InitializeComponent();
	}
}