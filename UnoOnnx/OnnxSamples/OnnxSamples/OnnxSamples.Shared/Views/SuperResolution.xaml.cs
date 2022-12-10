﻿using Microsoft.UI.Xaml.Controls;

using OnnxSamples.Models;

using System;
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

        public string[] EmbeddedResources { get; } = typeof(MainPage).Assembly.GetManifestResourceNames();

        public SuperResolution()
        {
            InitializeComponent();
            superResolution = new PytorchSuperResolution();
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

                var dialog = new ContentDialog();
                dialog.Content = result;
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
            await RunSuperResolutionAsync();
            runButton.IsEnabled = true;
        }

    }
}
