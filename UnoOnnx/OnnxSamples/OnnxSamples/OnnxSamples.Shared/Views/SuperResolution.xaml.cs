using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

using OnnxSamples.Models;

using System;
using System.IO;
using System.Threading.Tasks;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OnnxSamples.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SuperResolution : Page
    {
        PytorchSuperResolution superResolution;
        PytorchBertQnA bertQnA;

        public string[] EmbeddedResources { get; } = typeof(MainPage).Assembly.GetManifestResourceNames();

        public SuperResolution()
        {
            InitializeComponent();
            superResolution = new PytorchSuperResolution();
            bertQnA = new PytorchBertQnA();
            foreach (var item in EmbeddedResources)
            {
                Console.WriteLine(item);
            }
        }

        async Task RunSuperResolutionAsync()
        {
            try
            {
                var sampleImage = await superResolution.GetSampleImageAsync("fish.jpeg");
                var result = await superResolution.GetSuperResolutionImage(sampleImage, "fish.jpeg");
                var resultantImage = new Image();
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(new MemoryStream(result.Item2).AsRandomAccessStream());
                resultantImage.Source = bitmapImage;
                resultantImage.Stretch = Stretch.Fill;
                resultantImage.Height = Math.Sqrt(result.Item2.Length);

                var dialog = new ContentDialog();
                dialog.Content = resultantImage;
                dialog.CloseButtonText = "Done";

                var dialogResult = await dialog.ShowAsync();

            }
            catch (Exception exception)
            {

                var dialog = new ContentDialog();
                dialog.Content = $"ERROR:{exception.Message}";
                dialog.CloseButtonText = "Done";

                var dialogResult = await dialog.ShowAsync();
            }
        }

        async Task RunBertQnAAsync(string question , string context)
        {
            try
            {
                var result = await bertQnA.GetAnswerFromBert(question, context);
                var dialog = new ContentDialog();
                dialog.Title = $"Answer to {question}";
                dialog.Content = $"Answer is : {result.Item2}";
                dialog.CloseButtonText = "Done";

                var dialogResult = await dialog.ShowAsync();
            }
            catch (Exception exception)
            {

                var dialog = new ContentDialog();
                dialog.Content = $"ERROR:{exception.Message}";
                dialog.CloseButtonText = "Done";

                var dialogResult = await dialog.ShowAsync();
            }
        }


        private async void RunButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var runButton = sender as Button;
            runButton.IsEnabled = false;
            await RunBertQnAAsync("Who was Jim Henson?", "Jim Henson was a nice puppet");
            //await RunSuperResolutionAsync();
            runButton.IsEnabled = true;
        }

    }
}
