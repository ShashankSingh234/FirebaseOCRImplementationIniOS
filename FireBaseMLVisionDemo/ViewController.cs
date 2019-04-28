using System;
using Firebase.MLKit.Vision;
using Foundation;
using UIKit;
using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreGraphics;
using System.Drawing;

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

            //using (CGContext g = UIGraphics.GetCurrentContext())
            //{

            //    //set up drawing attributes
            //    g.SetLineWidth(10);
            //    UIColor.Blue.SetFill();
            //    UIColor.Red.SetStroke();

            //    //create geometry
            //    var path = new CGPath();

            //    path.AddLines(new CGPoint[]{
            //        new CGPoint (100, 200),
            //        new CGPoint (160, 100),
            //        new CGPoint (220, 200)});

            //    path.CloseSubpath();

            //    //add geometry to graphics context and draw it
            //    g.AddPath(path);
            //    g.DrawPath(CGPathDrawingMode.FillStroke);
            //}

            CapturePortionView.Layer.BorderColor = UIColor.Cyan.CGColor;
            CapturePortionView.Layer.BorderWidth = 2.0f;
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

            CameraView.BringSubviewToFront(CapturePortionView);

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
                CameraView.Hidden = CapturePortionView.Hidden = false;
                ImageView.Hidden = true;
                TakePhoto.SetTitle("Capture Image", UIControlState.Normal);
                return;
            }

            CameraView.Hidden = CapturePortionView.Hidden = true;
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

            image = CropImage(image, (float)CapturePortionView.Frame.Left, (float)(CapturePortionView.Frame.Top - CapturePortionView.Frame.Height - TopLayoutGuide.Length), (float)CapturePortionView.Frame.Width, (float)CapturePortionView.Frame.Height);

            ImageView.Image = image;

            var visionImage = new VisionImage(ImageView.Image);

            textRecognizer.ProcessImage(visionImage, HandleVisionTextRecognitionCallbackHandler);
        }

        private UIImage CropImage(UIImage sourceImage, float crop_x, float crop_y, float width, float height)
        {
            var imgSize = sourceImage.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);
            var drawRect = new RectangleF(-crop_x, -crop_y, (float)imgSize.Width, (float)imgSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return modifiedImage;
        }

        [Export("captureOutput:didCapturePhotoForResolvedSettings:")]
        public void DidCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
        {

        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            aVCaptureSession.StopRunning();
        }

    }
}
