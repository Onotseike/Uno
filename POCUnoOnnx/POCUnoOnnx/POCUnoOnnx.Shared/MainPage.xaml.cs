
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;

using Exception = System.Exception;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace POCUnoOnnx
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string currentFile;
        
        public string[] EmbeddedResources { get; set; } = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceNames();
        
        public string CurrentFile
        {
            get => currentFile;
            set
            {
                currentFile = value;
                using (var resource = typeof(MainPage).Assembly.GetManifestResourceStream(value))
                {
                    try
                    {
                        var reader = new StreamReader(resource);
                        content.Text = reader.ReadToEnd();
                    }
                    catch (Exception exception)
                    {
                        content.Text = $"Error: {exception.Message}";
                    }
                }
            }
        }

        MobileNetImageClassifier _classifier;
        
        public MainPage()
        {
            this.InitializeComponent();
            _classifier = new MobileNetImageClassifier(EmbeddedResources);
        }

        async Task RunInferenceAsync()
        {
            runClassifer.IsEnabled = false;

            try
            {
                var sampleImage = await _classifier.GetSampleImageAsync();
                var result = await _classifier.GetClassificationAsync(sampleImage);

                content.Text = $"The Result is: {result}";
            }
            catch (Exception exception)
            {
                content.Text = $"Error: {exception.Message}";
            }
            finally
            {
                runClassifer.IsEnabled = true;
            }
        }

        private void runClassifer_Click(object sender, RoutedEventArgs e)
        {
            _ = RunInferenceAsync();
        }

        public async Task<byte[]> GetBytesFromFile(StorageFile file)
        {
            var stream = await file.OpenStreamForReadAsync();
            byte[] bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        private async void selectImage_Click(object sender, RoutedEventArgs e)
        {
            runClassifer.IsEnabled = false;
            try
            {
                var captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
                captureUI.PhotoSettings.CroppedSizeInPixels = new Size(224, 224);

                var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (photo == null)
                {
                    return;
                }
                else
                {
                    var sourceImage = await GetBytesFromFile(photo);
                    var result = await _classifier.GetClassificationAsync(sourceImage);
                }
            }
            catch (Exception exception)
            {

                content.Text = $"Error: {exception.Message}";
            }
            finally
            {
                runClassifer.IsEnabled = true;
            }
        }
    }
}
