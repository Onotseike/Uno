using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Media;
using CommunityToolkit.WinUI.Helpers;
using Windows.Graphics.Imaging;
using Windows.Media.Core;

#if WINDOWS
using CommunityToolkit.WinUI.UI.Controls;
#else
using CommunityToolkit.WinUI.UI.Controls;
#endif

namespace CameraPreview.Presentation
{
    public sealed partial class MainPage : Page
    {

        #region Field(s)

        private VideoFrame _currentVideoFrame;
        private SoftwareBitmapSource _softwareBitmapSource;
        private SoftwareBitmap softwareBitmap;

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
        }
        #region Override(s)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UnsubscribeFromEvents();

            if (CameraPreviewControl != null)
            {
                CameraHelper cameraHelper = CameraPreviewControl.CameraHelper;
                CameraPreviewControl.PreviewFailed += CameraPreviewControl_PreviewFailed;
                await CameraPreviewControl.StartAsync(cameraHelper);
                
                CameraPreviewControl.CameraHelper.FrameArrived += CameraPreviewControl_FrameArrived;
            }

            _softwareBitmapSource = new SoftwareBitmapSource();

            await _softwareBitmapSource.SetBitmapAsync(softwareBitmap);

            CurrentFrameImage.Source = _softwareBitmapSource;

           
            
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            if (Frame?.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanUpAsync();
                deferral.Complete();
            }
        }

        private async void Application_Resuming(object? sender, object e)
        {
            if (CameraPreviewControl != null)
            {
                var cameraHelper = CameraPreviewControl.CameraHelper;
                CameraPreviewControl.PreviewFailed += CameraPreviewControl_PreviewFailed;
                await CameraPreviewControl.StartAsync(cameraHelper);
                CameraPreviewControl.CameraHelper.FrameArrived += CameraPreviewControl_FrameArrived;
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            await CleanUpAsync();
        }

        #endregion

        #region  Helper Method(s)

        public void CameraPreviewControl_FrameArrived(object sender, FrameEventArgs e)
        {
            _currentVideoFrame = e.VideoFrame;            
            softwareBitmap = e.VideoFrame.SoftwareBitmap;

            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
            }

        }

        public void CameraPreviewControl_PreviewFailed(object sender, PreviewFailedEventArgs e)
        {
            ErrorMessage.Text = e.Error;
        }

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            var softwareBitmap = _currentVideoFrame?.SoftwareBitmap;
            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                await _softwareBitmapSource.SetBitmapAsync(softwareBitmap);
                CurrentFrameImage.Source = _softwareBitmapSource;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (CameraPreviewControl != null)
            {
                if (CameraPreviewControl.CameraHelper != null)
                {
                    CameraPreviewControl.CameraHelper.FrameArrived -= CameraPreviewControl_FrameArrived;
                }

                CameraPreviewControl.PreviewFailed -= CameraPreviewControl_PreviewFailed;
            }
        }

        private async Task CleanUpAsync()
        {
            UnsubscribeFromEvents();

            if (CameraPreviewControl != null)
            {
                CameraPreviewControl.Stop();
                await CameraPreviewControl.CameraHelper.CleanUpAsync();
            }
        }

        #endregion
    }
}