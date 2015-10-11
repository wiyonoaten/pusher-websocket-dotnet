using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using PusherClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Android.Text.Method;
using System.Text;

namespace ExampleApplication.Android
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/icon",
		ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation 
		| global::Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
		private const string PUSHER_APP_KEY = "";
		private const string PUSHER_AUTH_HOST = "http://192.168.1.87:8887";

        private Pusher _pusher = null;
        private Channel _chatChannel = null;
        private PresenceChannel _presenceChannel = null;
        private string _name;

		private ViewGroup _layoutConnectDisconnect;
		private ViewGroup _layoutChatView;
		private ViewGroup _layoutSendMessage;
		private EditText _editName;
		private Button _buttonStart;
        private TextView _textConsole;
		private EditText _editMessage;
		private Button _buttonSend;

        private StringBuilder _sbMessages = new StringBuilder();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

			_layoutConnectDisconnect = FindViewById<RelativeLayout>(Resource.Id.LayoutConnectDisconnect);
            _layoutChatView = FindViewById<LinearLayout>(Resource.Id.LayoutChatView);
            _layoutSendMessage = FindViewById<RelativeLayout>(Resource.Id.LayoutSendMessage);
            _editName = FindViewById<EditText>(Resource.Id.EditName);
            _buttonStart = FindViewById<Button>(Resource.Id.ButtonStart);
            _textConsole = FindViewById<TextView>(Resource.Id.TextConsole);
            _editMessage = FindViewById<EditText>(Resource.Id.EditMessage);
			_buttonSend = FindViewById<Button>(Resource.Id.ButtonSend);

            SetViewAndChildrenEnabled(_layoutSendMessage, false);

            _buttonStart.Tag = true;
            _buttonStart.Text = GetString(Resource.String.ButtonConnect);

            _textConsole.MovementMethod = new ScrollingMovementMethod();

            _editMessage.Hint = "";

            _buttonStart.Click += (sender, args) =>
            {
                if ((bool)_buttonStart.Tag == true)
                {
                    if (!string.IsNullOrEmpty(_editName.Text))
                    {
                        SetViewAndChildrenEnabled(_layoutConnectDisconnect, false);
                        _buttonStart.Tag = false;
                        
                        _name = _editName.Text;

                        InitPusher();
                    }
                }
                else
                {
                    SetViewAndChildrenEnabled(_layoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(_layoutSendMessage, false);
                    _buttonStart.Tag = true;
                    _buttonStart.Text = GetString(Resource.String.ButtonConnect);
                    _editMessage.Hint = "";

                    UninitPusher();
                }
            };

			_buttonSend.Click += (sender, args) => 
			{
				if (_pusher.State != ConnectionState.Connected)
				{
					new AlertDialog.Builder(this)
						.SetMessage(GetString(Resource.String.AlertNotConnected))
						.Create()
						.Show();
					return;
				}

				if (!string.IsNullOrEmpty(_editMessage.Text))
				{
					_chatChannel.Trigger("client-my-event", new { message = _editMessage.Text, name = _name });
                    ConsoleWriteLine(_editMessage.Text);
                    _editMessage.Text = "";
                }
			};

            _textConsole.LayoutChange += (sender, args) =>
            {
                if (args.Bottom - args.Top != args.OldBottom - args.OldTop)
                {
                    ConsoleResetScroll();
                    ConsoleScrollToBottomIfRequired();
                }
            };
        }

        private static void SetViewAndChildrenEnabled(View view, bool enabled)
        {
            view.Enabled = enabled;
            if (view is ViewGroup)
            {
                ViewGroup viewGroup = (ViewGroup)view;
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    View child = viewGroup.GetChildAt(i);
                    SetViewAndChildrenEnabled(child, enabled);
                }
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
				RunOnUiThread(() =>
                {
                    _pusher.ConnectionStateChanged -= _pusher_ConnectionStateChanged;

                    Toast.MakeText(this, GetString(Resource.String.ToastDisconnected), ToastLength.Short).Show();

                    SetViewAndChildrenEnabled(_layoutConnectDisconnect, true);
                    SetViewAndChildrenEnabled(_layoutSendMessage, false);
                    _buttonStart.Text = GetString(Resource.String.ButtonConnect);
                    _editMessage.Hint = "";
                });
			}
        }

		private void pusher_Connected(object sender)
        {
            RunOnUiThread(() =>
            {
                _buttonStart.Text = GetString(Resource.String.ButtonDisconnect);
                _buttonStart.Enabled = true;
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
                SetViewAndChildrenEnabled(_layoutSendMessage, true);
                _editMessage.Hint = GetString(Resource.String.FormatHintEnterMessage, _name);
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
                _textConsole.Text = _sbMessages.ToString();
                ConsoleScrollToBottomIfRequired();
            });
        }

        private void ConsoleResetScroll()
        {
            _textConsole.ScrollTo(0, 0);
        }

        private void ConsoleScrollToBottomIfRequired()
        {
            int scroll_amount = (_textConsole.LineCount * _textConsole.LineHeight) - _textConsole.Height;
            if (scroll_amount > 0)
            {
                _textConsole.ScrollTo(0, scroll_amount);
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
