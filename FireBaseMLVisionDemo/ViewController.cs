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

namespace FireBaseMLVisionDemo
{
    public partial class ViewController : UIViewController, IAVCapturePhotoCaptureDelegate, IAVCaptureVideoDataOutputSampleBufferDelegate
    {
        AVCaptureSession aVCaptureSession;
        //AVCapturePhotoOutput aVCapturePhotoOutput;
        AVCaptureVideoDataOutput aVCaptureVideoDataOutput;
        AVCaptureVideoPreviewLayer aVCaptureVideoPreviewLayer;
        AVCaptureDevice backCamera;
        VisionImageMetadata metaData;
        //public CaptureVideoDelegate captureVideoDelegate;
        bool ignoreFirstFrame = true;
        //public AVCaptureVideoPreviewLayer previewLayer;
        //public AVCaptureSession captureSession;
        //public AVCaptureDevice captureDevice;
        //public AVCaptureDeviceInput captureDeviceInput;
        //public AVCaptureVideoDataOutput captureVideoOutput;

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

            CapturePortionView.Layer.BorderColor = UIColor.Cyan.CGColor;
            CapturePortionView.Layer.BorderWidth = 2.0f;
            // Perform any additional setup after loading the view, typically from a nib.

            //SetupVideoCaptureSession();
        }

        //public void SetupVideoCaptureSession()
        //{
        //    // Create notifier delegate class 
        //    captureVideoDelegate = new CaptureVideoDelegate();

        //    // Create capture session
        //    captureSession = new AVCaptureSession();
        //    captureSession.BeginConfiguration();
        //    captureSession.SessionPreset = AVCaptureSession.Preset640x480;

        //    // Create capture device
        //    captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);

        //    // Create capture device input
        //    NSError error;
        //    captureDeviceInput = new AVCaptureDeviceInput(captureDevice, out error);
        //    captureSession.AddInput(captureDeviceInput);

        //    // Create capture device output
        //    captureVideoOutput = new AVCaptureVideoDataOutput();
        //    captureVideoOutput.AlwaysDiscardsLateVideoFrames = true;
        //    // UPDATE: Wrong videosettings assignment
        //    //captureVideoOutput.VideoSettings.PixelFormat = CVPixelFormatType.CV32BGRA;
        //    // UPDATE Correct videosettings assignment
        //    var settings = new AVVideoSettingsUncompressed();
        //    settings.PixelFormatType = CVPixelFormatType.CV32BGRA;
        //    captureVideoOutput.WeakVideoSettings = settings.Dictionary;
        //    captureVideoOutput.MinFrameDuration = new CMTime(1, 30);
        //    DispatchQueue dispatchQueue = new DispatchQueue("VideoCaptureQueue");
        //    captureVideoOutput.SetSampleBufferDelegateQueue(this, dispatchQueue);
        //    captureSession.AddOutput(captureVideoOutput);

        //    // Create preview layer
        //    previewLayer = AVCaptureVideoPreviewLayer.FromSession(captureSession);
        //    previewLayer.Orientation = AVCaptureVideoOrientation.LandscapeLeft;
        //    previewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
        //    previewLayer.Frame = new RectangleF(0, 0, 1024, 768);
        //    this.View.Layer.AddSublayer(previewLayer);

        //    // Start capture session
        //    captureSession.CommitConfiguration();
        //    captureSession.StartRunning();
        //}


        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            aVCaptureSession = new AVCaptureSession();
            aVCaptureSession.SessionPreset = AVCaptureSession.PresetPhoto;

            //var availableDevice = AVCaptureDeviceDiscoverySession.Create(AVCaptureDeviceType.BuiltInWideAngleCamera, CMMediaType.Video, AVCaptureDevicePosition.Back);

            aVCaptureVideoPreviewLayer = new AVCaptureVideoPreviewLayer(aVCaptureSession);
            aVCaptureVideoPreviewLayer.VideoGravity = AVLayerVideoGravity.Resize;
            //aVCaptureVideoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
            CameraView.Layer.AddSublayer(aVCaptureVideoPreviewLayer);

            CameraView.BringSubviewToFront(CapturePortionView);

            //aVCaptureSession.StartRunning();

            aVCaptureVideoPreviewLayer.Frame = CameraView.Bounds;

            backCamera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            if (backCamera == null)
                TextView.Text = "Unable to access camera";

            NSError error;
            var input = new AVCaptureDeviceInput(backCamera, out error);

            //aVCapturePhotoOutput = new AVCapturePhotoOutput();
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
                //SetupLivePreview();
            }
        }

        partial void FlashButton_TouchUpInside(UIButton sender)
        {
            //if (!backCamera.HasFlash)
            //    return;

            //NSError error;
            //var lockForConfiguration = backCamera.LockForConfiguration(out error);

            //if (!lockForConfiguration)
            //    return;

            //if (CameraView.Hidden)
            //{
            //    FlashButton.SetTitle("On flash", UIControlState.Normal);
            //    backCamera.TorchMode = AVCaptureTorchMode.Off;
            //    backCamera.FlashMode = AVCaptureFlashMode.Off;
            //}
            //else 
            //{ 
            //    if (backCamera.FlashMode == AVCaptureFlashMode.Off)
            //    {
            //        FlashButton.SetTitle("Off flash", UIControlState.Normal);
            //        backCamera.TorchMode = AVCaptureTorchMode.On;
            //        backCamera.FlashMode = AVCaptureFlashMode.On;
            //    }
            //    else
            //    {
            //        FlashButton.SetTitle("On flash", UIControlState.Normal);
            //        backCamera.TorchMode = AVCaptureTorchMode.Off;
            //        backCamera.FlashMode = AVCaptureFlashMode.Off;
            //    }
            //}
            //backCamera.UnlockForConfiguration();
        }

        void SetupLivePreview()
        {
            //aVCaptureVideoPreviewLayer = new AVCaptureVideoPreviewLayer(aVCaptureSession);
            //aVCaptureVideoPreviewLayer.VideoGravity = AVLayerVideoGravity.Resize;
            //aVCaptureVideoPreviewLayer.Connection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
            //CameraView.Layer.AddSublayer(aVCaptureVideoPreviewLayer);

            //CameraView.BringSubviewToFront(CapturePortionView);

            //aVCaptureSession.StartRunning();

            //aVCaptureVideoPreviewLayer.Frame = CameraView.Bounds;
            //DispatchQueue.DefaultGlobalQueue.DispatchAsync(() => { aVCaptureSession.StartRunning(); });

            //DispatchQueue.MainQueue.DispatchAsync(() => { aVCaptureVideoPreviewLayer.Frame = CameraView.Bounds; });
        }

        private void HandleVisionTextRecognitionCallbackHandler(VisionText text, NSError error)
        {
            TextView.Text = error?.Description ?? text?.Text;
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

            //FlashButton_TouchUpInside(null);

            aVCaptureSession.StopRunning();
            //var settings = AVCapturePhotoSettings.FromFormat(NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
            //new object[]
            //{
            //    AVVideo.CodecJPEG,
            //},
            //new object[]
            //{
            //    AVVideo.CodecKey,
            //}));

            //aVCapturePhotoOutput.CapturePhoto(settings, this);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        [Export("captureOutput:didFinishProcessingPhoto:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            //var imageData = photo.FileDataRepresentation;
            //if (imageData == null)
            //    return;

            //var image = new UIImage(imageData);

            //image = CropImage(image, (float)CapturePortionView.Frame.Left, (float)(CapturePortionView.Frame.Top - CapturePortionView.Frame.Height - TopLayoutGuide.Length), (float)CapturePortionView.Frame.Width, (float)CapturePortionView.Frame.Height);

            //ImageView.Image = image;

            //var visionImage = new VisionImage(ImageView.Image);

            //textRecognizer.ProcessImage(visionImage, HandleVisionTextRecognitionCallbackHandler);
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

        [Export("captureOutput:didCapturePhotoForResolvedSettings:")]
        public void DidCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings)
        {
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            //aVCaptureSession.StopRunning();
        }

        [Export("captureOutput:didOutputSampleBuffer:fromConnection:")]
        public void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            //var pixelSample = sampleBuffer.GetImageBuffer() as CVPixelBuffer;
            //if (metaData == null)
            //{
            //    metaData = new VisionImageMetadata();

            //    var devicePosition = AVCaptureDevicePosition.Back;
            //    var deviceOrientation = UIDevice.CurrentDevice.Orientation;

            //    switch (deviceOrientation)
            //    {
            //        case UIDeviceOrientation.Portrait:
            //            metaData.Orientation = devicePosition == AVCaptureDevicePosition.Front ? VisionDetectorImageOrientation.LeftTop : VisionDetectorImageOrientation.RightTop;
            //            break;
            //        case UIDeviceOrientation.LandscapeLeft:
            //            metaData.Orientation = devicePosition == AVCaptureDevicePosition.Front ? VisionDetectorImageOrientation.BottomLeft : VisionDetectorImageOrientation.TopLeft;
            //            break;
            //        case UIDeviceOrientation.PortraitUpsideDown:
            //            metaData.Orientation = devicePosition == AVCaptureDevicePosition.Front ? VisionDetectorImageOrientation.RightBottom : VisionDetectorImageOrientation.LeftBottom;
            //            break;
            //        case UIDeviceOrientation.LandscapeRight:
            //            metaData.Orientation = devicePosition == AVCaptureDevicePosition.Front ? VisionDetectorImageOrientation.TopRight : VisionDetectorImageOrientation.BottomRight;
            //            break;
            //        case UIDeviceOrientation.FaceUp:
            //        case UIDeviceOrientation.FaceDown:
            //        case UIDeviceOrientation.Unknown:
            //            metaData.Orientation = VisionDetectorImageOrientation.LeftTop;
            //            break;
            //    }
            //}
            //var visionImage = new VisionImage(sampleBuffer);
            //sampleBuffer.Dispose();
            //visionImage.Metadata = metaData;
            //metaData.Dispose();
            //textRecognizer.ProcessImage(visionImage, HandleVisionTextRecognitionCallbackHandler);
            //visionImage.Dispose();

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

    //public class CaptureVideoDelegate : AVCaptureVideoDataOutputSampleBufferDelegate
    //{
    //    public CaptureVideoDelegate() : base()
    //    {
    //    }

    //    public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
    //    {
    //        // TODO: Implement buffer processing

    //        // Very important (buffer needs to be disposed or it will freeze)
    //        sampleBuffer.Dispose();
    //    }
    //}
}
