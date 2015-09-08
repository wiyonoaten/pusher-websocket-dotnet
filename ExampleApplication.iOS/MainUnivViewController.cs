using Foundation;
using Newtonsoft.Json.Linq;
using PusherClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ToastIOS;
using UIKit;

namespace ExampleApplication.iOS
{
    public partial class MainUnivViewController : UIViewController
    {
        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        private const string PUSHER_APP_KEY = "";
        private const string PUSHER_AUTH_HOST = "http://192.168.1.87:8887";

        private Pusher _pusher = null;
        private Channel _chatChannel = null;
        private PresenceChannel _presenceChannel = null;
        private string _name;

        private StringBuilder _sbMessages = new StringBuilder();

        public MainUnivViewController(IntPtr handle) : base(handle)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        #region View lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.

            SetViewAndChildrenEnabled(this.LayoutSendMessage, false);

            this.ButtonStart.Tag = 1;
            this.ButtonStart.SetTitle("Connect :)", UIControlState.Normal);

            this.EditMessage.Placeholder = "";

            this.ButtonStart.TouchUpInside += (sender, args) =>
            {
                if (this.ButtonStart.Tag == 1)
                {
                    if (!string.IsNullOrEmpty(this.EditName.Text))
                    {
                        SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, false);
                        this.ButtonStart.Tag = 0;

                        _name = this.EditName.Text;

                        InitPusher();
                    }
                }
                else
                {
                    SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(this.LayoutSendMessage, false);
                    this.ButtonStart.Tag = 1;
                    this.ButtonStart.SetTitle("Connect :)", UIControlState.Normal);
                    this.EditMessage.Placeholder = "";

                    UninitPusher();
                }
            };

            this.ButtonSend.TouchUpInside += (sender, args) =>
            {
                if (_pusher.State != ConnectionState.Connected)
                {
                    new UIAlertView("Error", "You are not (or no longer) connected!", null, "OK", null).Show();
                    return;
                }

                if (!string.IsNullOrEmpty(this.EditMessage.Text))
                {
                    _chatChannel.Trigger("client-my-event", new { message = this.EditMessage.Text, name = _name });
                    ConsoleWriteLine(this.EditMessage.Text);
                    this.EditMessage.Text = "";
                }
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
        }

        #endregion

        private static void SetViewAndChildrenEnabled(UIView view, bool enabled)
        {
            if (view is UIControl)
            {
                (view as UIControl).Enabled = enabled;
            }
            for (int i = 0; i < view.Subviews.Length; i++)
            {
                UIView child = view.Subviews[i];
                SetViewAndChildrenEnabled(child, enabled);
            }
        }

        #region Pusher Initiation / Connection

        private void InitPusher()
        {
            _pusher = new Pusher(PUSHER_APP_KEY, new PusherOptions()
            {
                Authorizer = new HttpAuthorizer(PUSHER_AUTH_HOST + "/auth/" + HttpUtility.UrlEncode(_name))
            });
            _pusher.Connected += pusher_Connected;
            _pusher.ConnectionStateChanged += _pusher_ConnectionStateChanged;
            _pusher.Connect();
        }

        private void UninitPusher()
        {
            _pusher.Connected -= pusher_Connected;
            _pusher.Disconnect();
        }

        private void _pusher_ConnectionStateChanged(object sender, ConnectionState state)
        {
            ConsoleWriteLine("Connection state: " + state.ToString());

            if (state == ConnectionState.Disconnected)
            {
                InvokeOnMainThread(() =>
                {
                    _pusher.ConnectionStateChanged -= _pusher_ConnectionStateChanged;

                    Toast.MakeText("Disconnected! Please start over again.", Toast.LENGTH_SHORT).Show();

                    SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(this.LayoutSendMessage, false);
                    this.ButtonStart.SetTitle("Connect :)", UIControlState.Normal);
                    this.EditMessage.Placeholder = "";
                });
            }
        }

        private void pusher_Connected(object sender)
        {
            InvokeOnMainThread(() =>
            {
                this.ButtonStart.SetTitle("Disconnect :(", UIControlState.Normal);
                this.ButtonStart.Enabled = true;
            });

            // Setup private channel
            _chatChannel = _pusher.Subscribe("private-channel");
            _chatChannel.Subscribed += _chatChannel_Subscribed;

            // Setup presence channel
            _presenceChannel = (PresenceChannel)_pusher.Subscribe("presence-channel");
            _presenceChannel.Subscribed += _presenceChannel_Subscribed;
            _presenceChannel.MemberAdded += _presenceChannel_MemberAdded;
            _presenceChannel.MemberRemoved += _presenceChannel_MemberRemoved;
        }

        #endregion

        #region Presence Channel Events

        private void _presenceChannel_Subscribed(object sender)
        {
            ListMembers();
        }

        private void _presenceChannel_MemberRemoved(object sender)
        {
            ListMembers();
        }

        private void _presenceChannel_MemberAdded(object sender)
        {
            ListMembers();
        }

        #endregion

        #region Chat Channel Events

        private void _chatChannel_Subscribed(object sender)
        {
            InvokeOnMainThread(() =>
            {
                SetViewAndChildrenEnabled(this.LayoutSendMessage, true);
                this.EditMessage.Placeholder = string.Format("Hi {0}! Type anything to chat and hit Send!", _name);
            });

            _chatChannel.Bind("client-my-event", (JObject data) =>
            {
                ConsoleWriteLine("[" + data["name"] + "] " + data["message"]);
            });
        }

        #endregion

        private void ConsoleWriteLine(string message)
        {
            _sbMessages.AppendLine(message);

            InvokeOnMainThread(() =>
            {
                this.TextConsole.Text = _sbMessages.ToString();
                ConsoleScrollToBottomIfRequired();
            });
        }

        private void ConsoleResetScroll()
        {
            this.TextConsole.ScrollRangeToVisible(new NSRange(0, 1));
        }

        private void ConsoleScrollToBottomIfRequired()
        {
            if (this.TextConsole.Text.Length > 0)
            {
                this.TextConsole.ScrollRangeToVisible(new NSRange(this.TextConsole.Text.Length - 1, 1));
            }    
        }

        private void ListMembers()
        {
            List<string> names = new List<string>();

            foreach (var mem in _presenceChannel.Members)
            {
                names.Add((string)mem.Value["name"]["Value"]);
            }

            ConsoleWriteLine("[MEMBERS] " + names.Aggregate((i, j) => i + ", " + j));
        }
    }
}