using Microsoft.UI.Xaml.Media.Imaging;

using SkiaSharp;

using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;

using Windows.Foundation;
using Windows.Media.Capture;

namespace Investigations.Presentation
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private void CameraBtn_Click(object sender, RoutedEventArgs e)
		{
			TakePhoto();
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

		private void ConvertPDF(ImageModel imageModel)
		{
			Task.Run(() => 
			{
				PdfDocument finalDocument = new PdfDocument();
				using( OCRProcessor processor = new OCRProcessor())
				{
					processor.ExternalEngine = new AzureOcrEngine();
                    FileStream imageStream = new FileStream(imageModel.OriginalImagePath, FileMode.Open);
					PdfDocument pdfDocument	= processor.PerformOCR(imageStream);
					MemoryStream saveStream = new MemoryStream();
					pdfDocument.Save(saveStream);
					pdfDocument.Close();
					PdfDocument.Merge(finalDocument, saveStream);
                }


				MemoryStream fileSave = new MemoryStream();
				finalDocument.Save(fileSave);
				fileSave.Position = 0;
				finalDocument.Close(true);

				
			});
		}
	}

	public class ImageModel
	{
		public string Name { get; set; } = string.Empty;
		public string LocalImagePath { get; set; } = string.Empty;
		public string OriginalImagePath { get; set; } = string.Empty;
	}
}