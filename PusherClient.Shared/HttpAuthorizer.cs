using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
#if (WINDOWS_UWP)
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PusherClient
{
    public class HttpAuthorizer: IAuthorizer
    {
        private Uri _authEndpoint;
        private EndPoint _proxyEndPoint;

        public HttpAuthorizer(string authEndpoint, EndPoint proxyEndPoint = null)
        {
            _authEndpoint = new Uri(authEndpoint);
            _proxyEndPoint = proxyEndPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="socketId"></param>
        /// <returns></returns>
        public virtual string Authorize(string channelName, string socketId)
        {
            string authToken = null;

            string data = GetPostData(channelName, socketId);
            string contentType = GetContentType();

            var proxyIpEndPoint = _proxyEndPoint as IPEndPoint;

#if (WINDOWS_UWP)
            var httpClientHandler = new HttpClientHandler();
            if (proxyIpEndPoint != null && httpClientHandler.SupportsProxy)
            {
                httpClientHandler.UseProxy = true;
                httpClientHandler.Proxy = new IpEndPointWebProxy(proxyIpEndPoint);
            }
            using (var httpClient = new HttpClient(httpClientHandler))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = _authEndpoint,
                    Method = HttpMethod.Post,
                };
                request.Content = new StringContent(data);
                ConfigureRequestHeaders(request.Content.Headers);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                authToken = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            }
#else
            using (var webClient = new System.Net.WebClient())
            {
                if (proxyIpEndPoint != null)
                {
                    webClient.Proxy = new WebProxy(proxyIpEndPoint.Address.ToString(), proxyIpEndPoint.Port);
                }
                webClient.Headers[HttpRequestHeader.ContentType] = contentType;
                ConfigureRequestHeaders(webClient.Headers);
                authToken = webClient.UploadString(_authEndpoint, "POST", data);
            }
#endif

            return authToken;
        }

        protected virtual string GetContentType()
        {
            return "application/x-www-form-urlencoded";
        }

        protected virtual string GetPostData(string channelName, string socketId)
        {
            return string.Format("channel_name={0}&socket_id={1}", channelName, socketId);
        }

#if (WINDOWS_UWP)
        protected virtual void ConfigureRequestHeaders(HttpContentHeaders headers)
        {
        }
#else
        protected virtual void ConfigureRequestHeaders(WebHeaderCollection headers)
        {
        }
#endif

#if (WINDOWS_UWP)
        private class IpEndPointWebProxy : IWebProxy
        {
            private IPEndPoint _proxyIpEndPoint;

            private ICredentials _creds;

            public IpEndPointWebProxy(IPEndPoint proxyIpEndPoint)
            {
                _proxyIpEndPoint = proxyIpEndPoint;

                _creds = CredentialCache.DefaultCredentials;
            }

            public ICredentials Credentials
            {
                get { return _creds; }
                set { _creds = value; }
            }

            public Uri GetProxy(Uri destination)
            {
                return new UriBuilder()
                {
                    Scheme = destination.Scheme,
                    Host = _proxyIpEndPoint.Address.ToString(),
                    Port = _proxyIpEndPoint.Port,
                }
                .Uri;
            }

            public bool IsBypassed(Uri host)
            {
                return false;
            }
        }
#endif
    }
}
