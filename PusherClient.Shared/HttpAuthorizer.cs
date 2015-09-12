using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
#if (WINDOWS_UWP)
using System.Net.Http;
#endif

namespace PusherClient
{
    public class HttpAuthorizer: IAuthorizer
    {
        private Uri _authEndpoint;
        public HttpAuthorizer(string authEndpoint)
        {
            _authEndpoint = new Uri(authEndpoint);
        }

        public string Authorize(string channelName, string socketId)
        {
            string authToken = null;

            string data = string.Format("channel_name={0}&socket_id={1}", channelName, socketId);
            string contentType = "application/x-www-form-urlencoded";

#if (WINDOWS_UWP)
            using (var httpClient = new HttpClient())
            {                
                var request = new HttpRequestMessage()
                {
                    RequestUri = _authEndpoint,
                    Method = HttpMethod.Post,
                };
                request.Content = new StringContent(data);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                authToken = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            }
#else
            using (var webClient = new System.Net.WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = contentType;
                authToken = webClient.UploadString(_authEndpoint, "POST", data);
            }
#endif

            return authToken;
        }
    }
}
