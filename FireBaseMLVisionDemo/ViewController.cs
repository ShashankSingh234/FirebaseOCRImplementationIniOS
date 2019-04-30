using System;
using Firebase.MLKit.Vision;
using Foundation;
using UIKit;
using AVFoundation;
using CoreFoundation;
using CoreMedia;
using CoreGraphics;
using System.Drawing;
using CoreVideo;
using System.Collections.Generic;

namespace FireBaseMLVisionDemo
{
    public partial class ViewController : UIViewController, IAVCapturePhotoCaptureDelegate, IAVCaptureVideoDataOutputSampleBufferDelegate
    {
        AVCaptureSession aVCaptureSession;
        AVCaptureVideoDataOutput aVCaptureVideoDataOutput;
        AVCaptureVideoPreviewLayer aVCaptureVideoPreviewLayer;
        AVCaptureDevice backCamera;
        bool ignoreFirstFrame = true;

        VisionApi vision;
        VisionTextRecognizer textRecognizer;

        List<string> detectedLocks;

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

            CapturePortionView.Layer.BorderColor = UIColor.Cyan.CGColor;
            CapturePortionView.Layer.BorderWidth = 2.0f;

            detectedLocks = new List<string>();
            // Perform any additional setup after loading the view, typically from a nib.
        }


        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            aVCaptureSession = new AVCaptureSession();
            aVCaptureSession.SessionPreset = AVCaptureSession.PresetPhoto;

            aVCaptureVideoPreviewLayer = new AVCaptureVideoPreviewLayer(aVCaptureSession);
            aVCaptureVideoPreviewLayer.VideoGravity = AVLayerVideoGravity.Resize;
            CameraView.Layer.AddSublayer(aVCaptureVideoPreviewLayer);

            CameraView.BringSubviewToFront(CapturePortionView);


            aVCaptureVideoPreviewLayer.Frame = CameraView.Bounds;

            backCamera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (backCamera == null)
                TextView.Text = "Unable to access camera";

            NSError error;
            var input = new AVCaptureDeviceInput(backCamera, out error);

            aVCaptureVideoDataOutput = new AVCaptureVideoDataOutput();
            var settings = new AVVideoSettingsUncompressed();
            settings.PixelFormatType = CVPixelFormatType.CV32BGRA;
            aVCaptureVideoDataOutput.WeakVideoSettings = settings.Dictionary;
            aVCaptureVideoDataOutput.AlwaysDiscardsLateVideoFrames = true;

            if (aVCaptureSession.CanAddInput(input) && aVCaptureSession.CanAddOutput(aVCaptureVideoDataOutput))
            {
                aVCaptureSession.AddInput(input);
                aVCaptureSession.AddOutput(aVCaptureVideoDataOutput);
                aVCaptureSession.CommitConfiguration();
                var queue = new DispatchQueue("Queue");
                aVCaptureVideoDataOutput.SetSampleBufferDelegateQueue(this, queue);
            }
        }

        partial void FlashButton_TouchUpInside(UIButton sender)
        {
            if (!backCamera.HasFlash)
                return;

            NSError error;
            var lockForConfiguration = backCamera.LockForConfiguration(out error);

            if (!lockForConfiguration)
                return;

            if (CameraView.Hidden)
            {
                FlashButton.SetTitle("On flash", UIControlState.Normal);
                backCamera.TorchMode = AVCaptureTorchMode.Off;
                backCamera.FlashMode = AVCaptureFlashMode.Off;
            }
            else 
            { 
                if (backCamera.FlashMode == AVCaptureFlashMode.Off)
                {
                    FlashButton.SetTitle("Off flash", UIControlState.Normal);
                    backCamera.TorchMode = AVCaptureTorchMode.On;
                    backCamera.FlashMode = AVCaptureFlashMode.On;
                }
                else
                {
                    FlashButton.SetTitle("On flash", UIControlState.Normal);
                    backCamera.TorchMode = AVCaptureTorchMode.Off;
                    backCamera.FlashMode = AVCaptureFlashMode.Off;
                }
            }
            backCamera.UnlockForConfiguration();
        }

        private void HandleVisionTextRecognitionCallbackHandler(VisionText text, NSError error)
        {
            string detectedText = text?.Text?.Replace(" ", string.Empty).Trim();
            if (detectedText?.Length == 8)
            {
                TextView.Text = detectedText;
                if (detectedLocks.Contains(detectedText))
                {
                    aVCaptureSession.StopRunning();
                    CameraView.Hidden = CapturePortionView.Hidden = FlashButton.Hidden = true;
                    ImageView.Hidden = false;
                    TakePhoto.SetTitle("Open Camera", UIControlState.Normal);
                    detectedLocks.Clear();
                    return;
                }
                detectedLocks.Add(detectedText);
            }
        }

        partial void DidCaptureImage(UIButton sender)
        {
            if(CameraView.Hidden)
            {
                CameraView.Hidden = CapturePortionView.Hidden = FlashButton.Hidden = false;
                ImageView.Hidden = true;
                TakePhoto.SetTitle("Capture Image", UIControlState.Normal);
                FlashButton.SetTitle("On flash", UIControlState.Normal);
                aVCaptureSession.StartRunning();

                return;
            }

            CameraView.Hidden = CapturePortionView.Hidden = FlashButton.Hidden = true;
            ImageView.Hidden = false;
            TakePhoto.SetTitle("Open Camera", UIControlState.Normal);

            aVCaptureSession.StopRunning();

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public UIImage ResizeImage(UIImage sourceImage, float width, float height)
        {
            UIGraphics.BeginImageContext(new SizeF(width, height));
            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
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

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            aVCaptureSession.StopRunning();
        }

        [Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
        public void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {

            try
            {
                if (connection.SupportsVideoOrientation)
                    connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;

                if (ignoreFirstFrame)
                {
                    ignoreFirstFrame = false;
                    return;
                }

                // Get the CoreVideo image
                using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
                {
                    // Lock the base address
                    pixelBuffer.Lock(CVPixelBufferLock.None);
                    // Get the number of bytes per row for the pixel buffer

                    var baseAddress = pixelBuffer.BaseAddress;
                    nint bytesPerRow = pixelBuffer.BytesPerRow;
                    nint width = pixelBuffer.Width;
                    nint height = pixelBuffer.Height;

                    var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
                    // Create a CGImage on the RGB colorspace from the configured parameter above
                    using (var cs = CGColorSpace.CreateDeviceRGB())
                    using (var context = new CGBitmapContext(baseAddress, width, height, 8, bytesPerRow, cs, flags))
                    using (var cgImage = context.ToImage())
                    {
                        pixelBuffer.Unlock(CVPixelBufferLock.None);
                        var CapturedImage = UIImage.FromImage(cgImage);

                        InvokeOnMainThread(() => {
                            CapturedImage = ResizeImage(CapturedImage, (float)View.Frame.Width, (float)View.Frame.Height);
                            CapturedImage = CropImage(CapturedImage, (float)CapturePortionView.Frame.Left, (float)CapturePortionView.Frame.Top, (float)CapturePortionView.Frame.Width, (float)CapturePortionView.Frame.Height);
                            ImageView.Image = CapturedImage;
                        });
                        var visionImage = new VisionImage(CapturedImage);
                        textRecognizer.ProcessImage(visionImage, HandleVisionTextRecognitionCallbackHandler);
                        visionImage.Dispose();
                    }
                }
            }
            finally
            {
                sampleBuffer.Dispose();
            }
        }
    }
}
