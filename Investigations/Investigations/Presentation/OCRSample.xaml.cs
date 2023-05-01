using Investigations.Business;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;

using SkiaSharp;

using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Investigations.Presentation
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class OCRSample : Microsoft.UI.Xaml.Controls.Page
	{
		public OCRSample()
		{
			this.InitializeComponent();
		}

		private async void CameraBtn_Click(object sender, RoutedEventArgs e)
		{
			//TakePhoto();
			// SelectPhoto();
			await GenerateChatCompletion();
		}

		private async Task GenerateChatCompletion()
		{
			var client = new OpenAPIClient();
			//var result = await client.CreateCompletions("This is a test in");
			var result = await client.TranscribeAudio();
			ocrResult.Text = result;

		}

		private async void TakePhoto()
		{
			try
			{
				var captureUI = new CameraCaptureUI();
				captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
				captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

				var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

				if (photo == null)
				{
					return;
				}
				else
				{
					var source = new BitmapImage(new Uri(photo.Path));
					using Stream sourceStream = await photo.OpenStreamForReadAsync();
					using SKBitmap sourceBitmap = SKBitmap.Decode(sourceStream);
					int height = Math.Min(794, sourceBitmap.Height);
					int width = Math.Min(794, sourceBitmap.Width);

					using SKBitmap resizedBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
					using SKImage resizedImage = SKImage.FromBitmap(resizedBitmap);
					//var filePath = Path.Combine(folder, Path.GetFileName(photo.Path));
					using (SKData data = resizedImage.Encode())
					{
						File.WriteAllBytes(photo.Path, data.ToArray());
					}

					ImageModel model = new ImageModel() { Name = Path.GetFileName(photo.Path), OriginalImagePath = photo.Path };

					//ImageViewControl.Source = source;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		private async void SelectPhoto()
		{
			try
			{
				var filePicker = new FileOpenPicker();
				filePicker.ViewMode = PickerViewMode.Thumbnail;
				filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
				filePicker.FileTypeFilter.AddRange(new string[] { ".jpg", ".jpeg", ".png" });
				var photo = await filePicker.PickSingleFileAsync();
				if (photo == null)
				{
					return;
				}
				else
				{

					var localPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Path.GetFileName(photo.Path));
					using Stream sourceStream = await photo.OpenStreamForReadAsync();

					await ConvertPDF(sourceStream);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		private async Task ConvertPDF(Stream imageStream)
		{
			var engine = new AzureOcrEngine();
			engine.Authenticate();
			var readResult = await engine.ReadFileUrl(engine.client, imageStream);
			ocrResult.TextWrapping = TextWrapping.Wrap;

			var texts = readResult.Lines.Select(line => line.Text).ToList();
            foreach (var line in readResult.Lines)
            {
                var count = line.BoundingBox.Count;
            }

            foreach (var text in texts)
			{
				var newParagraph = new Paragraph();
				newParagraph.Inlines.Add(new Run() { Text = text });
				ocrResult.Text += text + "\n";
			}
		}


	}
}
