// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ExampleApplication.iOS
{
	[Register ("MainUnivViewController")]
	partial class MainUnivViewController
	{
		[Outlet]
		UIKit.UIButton ButtonSend { get; set; }

		[Outlet]
		UIKit.UIButton ButtonStart { get; set; }

		[Outlet]
		UIKit.UITextField EditMessage { get; set; }

		[Outlet]
		UIKit.UITextField EditName { get; set; }

		[Outlet]
		UIKit.UITextView LabelMessages { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EditName != null) {
				EditName.Dispose ();
				EditName = null;
			}

			if (ButtonStart != null) {
				ButtonStart.Dispose ();
				ButtonStart = null;
			}

			if (LabelMessages != null) {
				LabelMessages.Dispose ();
				LabelMessages = null;
			}

			if (EditMessage != null) {
				EditMessage.Dispose ();
				EditMessage = null;
			}

			if (ButtonSend != null) {
				ButtonSend.Dispose ();
				ButtonSend = null;
			}
		}
	}
}
