using PusherClient;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExampleApplication.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string PUSHER_APP_KEY = "";
        private const string PUSHER_AUTH_HOST = "http://192.168.1.87:8887";

        private Pusher _pusher = null;
        private Channel _chatChannel = null;
        private PresenceChannel _presenceChannel = null;
        private string _name;

        private StringBuilder _sbMessages = new StringBuilder();

        public MainPage()
        {
            this.InitializeComponent();

            SetViewAndChildrenEnabled(this.LayoutSendMessage, false);

            this.ButtonStart.Tag = true;
            this.ButtonStart.Content = GetString("StringButtonConnect");

            this.EditMessage.PlaceholderText = "";

            this.EditName.KeyUp += (sender, args) =>
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    FireClickButton(this.ButtonStart);
                }
            };

            this.ButtonStart.Click += (sender, args) =>
            {
                if ((bool)this.ButtonStart.Tag == true)
                {
                    if (!string.IsNullOrEmpty(this.EditName.Text))
                    {
                        SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, false);
                        this.ButtonStart.Tag = false;

                        _name = this.EditName.Text;

                        InitPusher();
                    }
                }
                else
                {
                    SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(this.LayoutSendMessage, false);
                    this.ButtonStart.Tag = true;
                    this.ButtonStart.Content = GetString("StringButtonConnect");
                    this.EditMessage.PlaceholderText = "";

                    UninitPusher();
                }
            };

            this.EditMessage.KeyUp += (sender, args) =>
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    FireClickButton(this.ButtonSend);
                }
            };

            this.ButtonSend.Click += (sender, args) =>
            {
                if (_pusher.State != ConnectionState.Connected)
                {
                    var _ = new MessageDialog("Error", GetString("StringAlertNotConnected"))
                        .ShowAsync();
                    return;
                }

                if (!string.IsNullOrEmpty(this.EditMessage.Text))
                {
                    _chatChannel.Trigger("client-my-event", new { message = this.EditMessage.Text, name = _name });
                    ConsoleWriteLine(this.EditMessage.Text);
                    this.EditMessage.Text = "";
                }
            };

            this.TextConsole.SizeChanged += (sender, args) =>
            {
                if (args.NewSize != args.PreviousSize)
                {
                    ConsoleResetScroll();
                    ConsoleScrollToBottomIfRequired();
                }
            };
        }

        private static string GetString(string resKey)
        {
            if (!Application.Current.Resources.ContainsKey(resKey))
            {
                return null;
            }
            return (string) Application.Current.Resources[resKey];
        }

        private void RunOnUiThread(Action action)
        {
            var _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        private static void SetViewAndChildrenEnabled(UIElement view, bool enabled)
        {
            if (view is Control)
            {
                (view as Control).IsEnabled = enabled;
            }
            if (view is Panel)
            {
                Panel panel = (Panel)view;
                foreach (UIElement child in panel.Children)
                {
                    SetViewAndChildrenEnabled(child, enabled);
                }
            }
        }

        private void ShowToast(string message, bool isShort)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList textElements = toastXml.GetElementsByTagName("text");
            textElements[0].AppendChild(toastXml.CreateTextNode(message));
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml)
            {
                ExpirationTime = DateTimeOffset.Now.AddSeconds(isShort ? 5000 : 20000),
            });
        }

        private void FireClickButton(Button button)
        {
            InputPane.GetForCurrentView().TryHide();

            (new ButtonAutomationPeer(button)
                        .GetPattern(PatternInterface.Invoke) as IInvokeProvider)
                        .Invoke();
        }

        #region Pusher Initiation / Connection

        private void InitPusher()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.87"), 8888);
            _pusher = new Pusher(PUSHER_APP_KEY, new PusherOptions()
            {
                Authorizer = new HttpAuthorizer(PUSHER_AUTH_HOST + "/auth/" + WebUtility.UrlEncode(_name), proxyEndPoint),
                ProxyEndPoint = proxyEndPoint,
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
                RunOnUiThread(() =>
                {
                    _pusher.ConnectionStateChanged -= _pusher_ConnectionStateChanged;

                    ShowToast(GetString("StringToastDisconnected"), true);

                    SetViewAndChildrenEnabled(this.LayoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(this.LayoutSendMessage, false);
                    this.ButtonStart.Content = GetString("StringButtonConnect");
                    this.EditMessage.PlaceholderText = "";
                });
            }
        }

        private void pusher_Connected(object sender)
        {
            RunOnUiThread(() =>
            {
                this.ButtonStart.Content = GetString("StringButtonDisconnect");
                this.ButtonStart.IsEnabled = true;
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

        private void _presenceChannel_MemberRemoved(object sender, string id)
        {
            ListMembers();
        }

        private void _presenceChannel_MemberAdded(object sender, string id)
        {
            ListMembers();
        }

        #endregion

        #region Chat Channel Events

        private void _chatChannel_Subscribed(object sender)
        {
            RunOnUiThread(() =>
            {
                SetViewAndChildrenEnabled(this.LayoutSendMessage, true);
                this.EditMessage.PlaceholderText = string.Format(GetString("StringFormatHintEnterMessage"), _name);
            });

            _chatChannel.Bind("client-my-event", (dynamic data) =>
            {
                ConsoleWriteLine("[" + data.name + "] " + data.message);
            });
        }

        #endregion

        private void ConsoleWriteLine(string message)
        {
            _sbMessages.AppendLine(message);

            RunOnUiThread(() =>
            {
                this.TextConsole.Text = _sbMessages.ToString();
                ConsoleScrollToBottomIfRequired();
            });
        }

        private void ConsoleResetScroll()
        {
            this.ScrollConsole.ChangeView(0.0, 0.0, null);
        }

        private void ConsoleScrollToBottomIfRequired()
        {
            double scroll_amount = (this.TextConsole.ActualHeight) - this.ScrollConsole.ActualHeight;
            if (scroll_amount > 0.0)
            {
                this.ScrollConsole.ChangeView(0.0, scroll_amount, null);
            }
        }

        private void ListMembers()
        {
            List<string> names = new List<string>();

            foreach (var mem in _presenceChannel.Members)
            {
                names.Add((string)mem.Value.name.Value);
            }

            ConsoleWriteLine("[MEMBERS] " + names.Aggregate((i, j) => i + ", " + j));
        }
    }
}
