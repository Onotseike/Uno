using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
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
				using (OCRProcessor processor = new OCRProcessor())
				{
					processor.ExternalEngine = new AzureOcrEngine();
					FileStream imageStream = new FileStream(imageModel.OriginalImagePath, FileMode.Open);
					PdfDocument pdfDocument = processor.PerformOCR(imageStream);
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
}
