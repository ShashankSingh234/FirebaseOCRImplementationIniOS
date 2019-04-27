// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace FireBaseMLVisionDemo
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView CameraView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton TakePhoto { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TextView { get; set; }

        [Action ("DidCaptureImage:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DidCaptureImage (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (CameraView != null) {
                CameraView.Dispose ();
                CameraView = null;
            }

            if (ImageView != null) {
                ImageView.Dispose ();
                ImageView = null;
            }

            if (TakePhoto != null) {
                TakePhoto.Dispose ();
                TakePhoto = null;
            }

            if (TextView != null) {
                TextView.Dispose ();
                TextView = null;
            }
        }
    }
}