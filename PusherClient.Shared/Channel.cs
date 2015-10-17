using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PusherClient
{
    public delegate void SubscriptionEventHandler(object sender);
    public delegate void FailedSubscriptionEventHandler(object sender, ErrorCodes ErrorCode, string errorMessage);

    public class Channel : EventEmitter
    {
        private Pusher _pusher = null;
        private bool _isSubscribed = false;

        public event SubscriptionEventHandler Subscribed;
        public event FailedSubscriptionEventHandler FailedToSubscribe;

        public string Name = null;

        public bool IsSubscribed
        {
            get
            {
                return _isSubscribed;
            }
        }

        public Channel(string channelName, Pusher pusher)
        {
            _pusher = pusher;
            this.Name = channelName;
        }

        internal virtual void SubscriptionSucceeded(string data)
        {
            _isSubscribed = true;

            if(Subscribed != null)
                Subscribed(this);
        }

        internal void SubscriptionFailed(ErrorCodes errorCode, string errorMessage)
        {
            if (FailedToSubscribe != null)
                FailedToSubscribe(this, errorCode, errorMessage);
        }

        public void Unsubscribe()
        {
            _isSubscribed = false;
            _pusher.Unsubscribe(this.Name);
        }

        public void Trigger(string eventName, object obj)
        {
            _pusher.Trigger(this.Name, eventName, obj);
        }
    }
}
