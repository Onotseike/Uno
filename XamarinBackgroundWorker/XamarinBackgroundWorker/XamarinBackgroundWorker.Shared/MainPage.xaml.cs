using CommunityToolkit.Mvvm.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XamarinBackgroundWorker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Create a BackgroundWorker instance
        private BackgroundWorker worker = new BackgroundWorker();

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = App.Current.Services.GetService(typeof(MainPageViewModel));

            // Set the properties of the BackgroundWorker
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            // Handle the events of the BackgroundWorker
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }


        private void StartButton_Clicked(object sender, EventArgs e)
        {
            // Start the BackgroundWorker
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
                StatusLabel.Text = "Working...";
            }
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            // Cancel the BackgroundWorker
            if (worker.IsBusy)
            {
                worker.CancelAsync();
                StatusLabel.Text = "Cancelling...";
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Perform the task on a separate thread
            for (int i = 0; i <= 100; i++)
            {
                // Check for cancellation
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                // Simulate some work
                System.Threading.Thread.Sleep(100);

                // Report progress
                worker.ReportProgress(i);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update the UI with the progress on the main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                ProgressBar.Progress = e.ProgressPercentage / 100.0;
                ProgressLabel.Text = $"{e.ProgressPercentage}%";
            });
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Handle the completion or cancellation of the task on the main thread
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e.Cancelled)
                {
                    StatusLabel.Text = "Cancelled";
                }
                else if (e.Error != null)
                {
                    StatusLabel.Text = "Error: " + e.Error.Message;
                }
                else
                {
                    StatusLabel.Text = "Done";
                }
            });
        }
    }
}
