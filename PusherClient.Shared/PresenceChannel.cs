﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PusherClient
{
#if (__IOS__)
    using dynamic = JObject;
#endif

    public delegate void MemberEventHandler(object sender, string id);

    public class PresenceChannel : PrivateChannel
    {
        public Dictionary<string, dynamic> Members = new Dictionary<string, dynamic>();

        public event MemberEventHandler MemberAdded;
        public event MemberEventHandler MemberRemoved;

        public PresenceChannel(string channelName, Pusher pusher) : base(channelName, pusher) { }

        #region Internal Methods

        internal override void SubscriptionSucceeded(string data)
        {
            Members = ParseMembersList(data);
            base.SubscriptionSucceeded(data);
        }

        internal void AddMember(string data)
        {
            var member = ParseMember(data);

            if (!Members.ContainsKey(member.Key))
                Members.Add(member.Key, member.Value);
            else
                Members[member.Key] = member.Value;

            if (MemberAdded != null)
                MemberAdded(this, member.Key);
        }

        internal void RemoveMember(string data)
        {
            var member = ParseMember(data);

            if (Members.ContainsKey(member.Key))
            {
                Members.Remove(member.Key);

                if (MemberRemoved != null)
                    MemberRemoved(this, member.Key);
            }
        }

        #endregion

        #region Private Methods

        private Dictionary<string, dynamic> ParseMembersList(string data)
        {
            Dictionary<string, dynamic> members = new Dictionary<string, dynamic>();

            var dataAsObj = JsonConvert.DeserializeObject<dynamic>(data);

#if (__IOS__)
            var memberCount = (int)dataAsObj["presence"]["count"];
#else
            var memberCount = (int)dataAsObj.presence.count;
#endif

            for (int i = 0; i < memberCount; i++)
            {
#if (__IOS__)
                var id = (string)dataAsObj["presence"]["ids"][i];
                var val = (dynamic)dataAsObj["presence"]["hash"][id];
#else
                var id = (string)dataAsObj.presence.ids[i];
                var val = (dynamic)dataAsObj.presence.hash[id];
#endif
                members.Add(id, val);
            }

            return members;
        }

        private KeyValuePair<string, dynamic> ParseMember(string data)
        {
            var dataAsObj = JsonConvert.DeserializeObject<dynamic>(data);

#if (__IOS__)
            var id = (string)dataAsObj["user_id"];
            var val = (dynamic)dataAsObj["user_info"];
#else
            var id = (string)dataAsObj.user_id;
            var val = (dynamic)dataAsObj.user_info;
#endif

            return new KeyValuePair<string, dynamic>(id, val);
        }

#endregion

    }
}
