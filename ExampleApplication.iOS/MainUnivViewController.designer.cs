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
		UIKit.UIStackView LayoutConnectDisconnect { get; set; }

		[Outlet]
		UIKit.UIStackView LayoutSendMessage { get; set; }

		[Outlet]
		UIKit.UITextView TextConsole { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LayoutConnectDisconnect != null) {
				LayoutConnectDisconnect.Dispose ();
				LayoutConnectDisconnect = null;
			}

			if (ButtonSend != null) {
				ButtonSend.Dispose ();
				ButtonSend = null;
			}

			if (ButtonStart != null) {
				ButtonStart.Dispose ();
				ButtonStart = null;
			}

			if (EditMessage != null) {
				EditMessage.Dispose ();
				EditMessage = null;
			}

			if (LayoutSendMessage != null) {
				LayoutSendMessage.Dispose ();
				LayoutSendMessage = null;
			}

			if (EditName != null) {
				EditName.Dispose ();
				EditName = null;
			}

			if (TextConsole != null) {
				TextConsole.Dispose ();
				TextConsole = null;
			}
		}
	}
}
