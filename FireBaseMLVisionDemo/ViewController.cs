using System;
using Firebase.MLKit.Vision;
using Foundation;
using UIKit;
using AVFoundation;
using CoreFoundation;
using CoreMedia;

namespace FireBaseMLVisionDemo
{
    public partial class ViewController : UIViewController, IAVCapturePhotoCaptureDelegate
    {
        AVCaptureSession aVCaptureSession;
        AVCapturePhotoOutput aVCapturePhotoOutput;
        AVCaptureVideoPreviewLayer aVCaptureVideoPreviewLayer;

        VisionApi vision;
        VisionTextRecognizer textRecognizer;
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            vision = VisionApi.Create();
            textRecognizer = vision.GetOnDeviceTextRecognizer();

            TakePhoto.SetTitle("Open camera", UIControlState.Normal);

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            aVCaptureSession = new AVCaptureSession();
            aVCaptureSession.SessionPreset = AVCaptureSession.PresetMedium;

            var backCamera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (backCamera == null)
                TextView.Text = "Unable to access camera";

            NSError error;
            var input = new AVCaptureDeviceInput(backCamera, out error);

            aVCapturePhotoOutput = new AVCapturePhotoOutput();

            if(aVCaptureSession.CanAddInput(input) && aVCaptureSession.CanAddOutput(aVCapturePhotoOutput))
            {
                aVCaptureSession.AddInput(input);
                aVCaptureSession.AddOutput(aVCapturePhotoOutput);
                SetupLivePreview();
            }
        }

        void SetupLivePreview()
        {
            aVCaptureVideoPreviewLayer = new AVCaptureVideoPreviewLayer(aVCaptureSession);
            aVCaptureVideoPreviewLayer.VideoGravity = AVLayerVideoGravity.Resize;
            aVCaptureVideoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
            CameraView.Layer.AddSublayer(aVCaptureVideoPreviewLayer);

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() => { aVCaptureSession.StartRunning(); });

            DispatchQueue.MainQueue.DispatchAsync(() => { aVCaptureVideoPreviewLayer.Frame = CameraView.Bounds; });
        }

        private void HandleVisionTextRecognitionCallbackHandler(VisionText text, NSError error)
        {
            TextView.Text = error?.Description ?? text?.Text;
        }

        partial void DidCaptureImage(UIButton sender)
        {
            if(CameraView.Hidden)
            {
                CameraView.Hidden = false;
                ImageView.Hidden = true;
                TakePhoto.SetTitle("Capture Image", UIControlState.Normal);
                return;
            }

            CameraView.Hidden = true;
            ImageView.Hidden = false;
            TakePhoto.SetTitle("Open Camera", UIControlState.Normal);

            var settings = AVCapturePhotoSettings.FromFormat(NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
            new object[]
            {
                AVVideo.CodecJPEG,
            },
            new object[]
            {
                AVVideo.CodecKey,
            }));

            aVCapturePhotoOutput.CapturePhoto(settings, this);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        [Export("captureOutput:didFinishProcessingPhoto:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            var imageData = photo.FileDataRepresentation;
            if (imageData == null)
                return;

            var image = new UIImage(imageData);
            ImageView.Image = image;

            var visionImage = new VisionImage(ImageView.Image);

            textRecognizer.ProcessImage(visionImage, HandleVisionTextRecognitionCallbackHandler);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            aVCaptureSession.StopRunning();
        }

    }
}
