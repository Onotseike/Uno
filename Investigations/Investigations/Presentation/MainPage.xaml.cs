namespace Investigations.Presentation
{
	public sealed partial class MainPage : Microsoft.UI.Xaml.Controls.Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected)
			{
				//contentFrame.Navigate(typeof(SettingsPage));
			}
			else
			{
				var selectedItem = (NavigationViewItem)args.SelectedItem;
				string selectedItemTag = ((string)selectedItem.Tag) ?? string.Empty.ToString();
				sender.Header = selectedItemTag;
				switch (selectedItemTag)
				{
					case "OCRSample":
						contentFrame.Navigate(typeof(OCRSample));
						break;
					case "Image to PDF":
						// contentFrame.Navigate(typeof(ImageToPdf));
						break;
					default: break;
				}
			}

		}
	}

	public class ImageModel
	{
		public string Name { get; set; } = string.Empty;
		public string LocalImagePath { get; set; } = string.Empty;
		public string OriginalImagePath { get; set; } = string.Empty;
	}
}