// WorkMode
#define _EnableWorkMode1
#define _EnableWorkMode2
//#define _EnableWorkMode3

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace com.net
{
    public class HttpClient : IDisposable
    {
        private string _Accept = null;

        private bool _AllowAutoRedirect = false;

        private bool _AllowCookie = false;

        private HttpRequestCachePolicy _CachePolicy = null;

        private string _ContentType = null;

        private CookieCollection _Cookies = null;

        private ICredentials _Credentials = null;

        private bool _KeepAlive = false;

        private WebProxy _Proxy = null;

        private long[] _Range = null;

        private string _Referer = null;

        private Dictionary<string, string> _RequestHeaders = null;

        private int _Timeout = 10000;

        private string _UserAgent = null;

        public HttpClient()
        {
        }

        /// <summary>
        /// 允許轉址
        /// </summary>
        public bool AllowAutoRedirect
        {
            set { _AllowAutoRedirect = value; }
        }

        public bool AllowCookie
        {
            get { return _AllowCookie; }
            set { _AllowCookie = value; }
        }

        public HttpRequestCachePolicy CachePolicy
        {
            set { _CachePolicy = value; }
        }

        public string ContentType
        {
            get { return _ContentType; }
        }

        public CookieCollection Cookies
        {
            get
            {
                if (_AllowCookie)
                    return _Cookies;
                return null;
            }
        }

        public ICredentials Credentials
        {
            get { return _Credentials; }
            set { _Credentials = value; }
        }

        public bool KeepAlive
        {
            set { _KeepAlive = value; }
        }

        public string Proxy
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    _Proxy = null;
                else
                    _Proxy = new WebProxy(value);
            }
        }

        public string Referer
        {
            set { _Referer = value; }
        }

        public Dictionary<string, string> RequestHeaders
        {
            get
            {
                if (_RequestHeaders == null)
                    _RequestHeaders = new Dictionary<string, string>();
                return _RequestHeaders;
            }
        }

        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }

        public string UserAgent
        {
            set { _UserAgent = value; }
        }

        private string Accept
        {
            set { _Accept = value; }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

#if _EnableWorkMode1

        private WebHeaderCollection _ResponseHeaders = null;

        private MemoryStream _ResponseStream = null;

        private string _ResponseText = null;

        private HttpStatusCode _StatusCode = HttpStatusCode.OK;

        private Encoding _TextEncoding = null;

        public WebHeaderCollection ResponseHeaders
        {
            get { return _ResponseHeaders; }
        }

        public MemoryStream ResponseStream
        {
            get { return _ResponseStream; }
        }

        public string ResponseText
        {
            get { return _ResponseText; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _StatusCode; }
        }

        public Encoding TextEncoding
        {
            get
            {
                if (_TextEncoding == null)
                    _TextEncoding = Encoding.Default;
                return _TextEncoding;
            }
            set { _TextEncoding = value; }
        }

        public HttpStatusCode Get(string strUrl,
            Dictionary<string, string> formData = null)
        {
            try
            {
                Uri uri = new Uri(strUrl);
                HttpWebRequest hwReq = CreateWebRequest(uri, "GET", formData);

                return ProcessRequest(hwReq);
            }
            catch (WebException ex)
            {
                switch (ex.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        if (ex.Response != null)
                            _StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
                        break;

                    case WebExceptionStatus.Timeout:
                        _StatusCode = HttpStatusCode.RequestTimeout;
                        break;

                    default:
                        throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _StatusCode;
        }

        public HttpStatusCode Post(string strUrl,
            Dictionary<string, string> formData)
        {
            try
            {
                Uri uri = new Uri(strUrl);
                HttpWebRequest hwReq = CreateWebRequest(uri, "POST", formData);

                return ProcessRequest(hwReq);
            }
            catch (WebException ex)
            {
                switch (ex.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        if (ex.Response != null)
                            _StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
                        break;

                    case WebExceptionStatus.Timeout:
                        _StatusCode = HttpStatusCode.RequestTimeout;
                        break;

                    default:
                        throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _StatusCode;
        }

        protected virtual HttpStatusCode ProcessRequest(HttpWebRequest hwReq)
        {
            using (HttpWebResponse hwRes = hwReq.GetResponse() as HttpWebResponse)
            {
                _ContentType = hwRes.ContentType;
                if (_AllowCookie)
                    _Cookies = hwRes.Cookies;
                if (hwRes.SupportsHeaders)
                    _ResponseHeaders = hwRes.Headers;

                if ((_ContentType != null) && (_ContentType.Substring(0, 4) == "text"))
                {
                    _ResponseStream = null;
                    using (StreamReader reader = new StreamReader(hwRes.GetResponseStream(), TextEncoding))
                    {
                        _ResponseText = reader.ReadToEnd();
                    }
                }
                else
                {
                    _ResponseText = null;
                    MemoryStream content = new MemoryStream();
                    using (Stream responseStream = hwRes.GetResponseStream())
                    {
                        responseStream.CopyTo(content);
                    }
                    _ResponseStream = content;
                }
            }
            _StatusCode = HttpStatusCode.OK;
            return _StatusCode;
        }

#endif

#if _EnableWorkMode2

        public async Task<MemoryStream> GetAsync(string strUrl,
            Dictionary<string, string> formData = null)
        {
            Uri uri = new Uri(strUrl);
            HttpWebRequest hwReq = CreateWebRequest(uri, "GET", formData);

            return await ProcessRequestAsync(hwReq);
        }

        public async Task<MemoryStream> PostAsync(string strUrl,
            Dictionary<string, string> formData)
        {
            Uri uri = new Uri(strUrl);
            HttpWebRequest hwReq = CreateWebRequest(uri, "POST", formData);

            return await ProcessRequestAsync(hwReq);
        }

        protected virtual async Task<MemoryStream> ProcessRequestAsync(HttpWebRequest hwReq)
        {
            // The downloaded resource ends up in the variable named content.
            MemoryStream content = new MemoryStream();

            // Send the request to the Internet resource and wait for the response.
            using (WebResponse response = await hwReq.GetResponseAsync())
            {
                // Get the data stream that is associated with the specified url.
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Read the bytes in responseStream and copy them to content.
                    await responseStream.CopyToAsync(content);
                }
            }
            // Return the result as a byte array.
            return content;
        }

#endif

#if _EnableWorkMode3

        public IAsyncResult GetAsync(string strUrl,
            Dictionary<string, string> formData = null,
            AsyncCallback callback = null)
        {
            Uri uri = new Uri(strUrl);
            HttpWebRequest hwReq = CreateWebRequest(uri, "GET", formData);

            if (callback == null)
                callback = new AsyncCallback(ProcessRequestAsync);

            return hwReq.BeginGetResponse(callback, hwReq);
        }

        public IAsyncResult PostAsync(string strUrl,
            Dictionary<string, string> formData,
            AsyncCallback callback = null)
        {
            Uri uri = new Uri(strUrl);
            HttpWebRequest hwReq = CreateWebRequest(uri, "POST", formData);

            if (callback == null)
                callback = new AsyncCallback(ProcessRequestAsync);

            return hwReq.BeginGetResponse(callback, hwReq);
        }


        protected virtual void ProcessRequestAsync(IAsyncResult asyncResult)
        {
            HttpWebRequest hwReq = asyncResult.AsyncState as HttpWebRequest;
            using (HttpWebResponse hwRes = hwReq.EndGetResponse(asyncResult) as HttpWebResponse)
            { 
                //TODO: ?
            }     
        }

#endif

        public void SetCachePolicy(HttpRequestCacheLevel CacheLevel)
        {
            _CachePolicy = new HttpRequestCachePolicy(CacheLevel);
        }

        public void SetDefaultNetworkCredentials()
        {
            _Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        public void SetDefaultProxy()
        {
            //過時用法?
            //_Proxy = WebProxy.GetDefaultProxy() as WebProxy;

            if (WebRequest.DefaultWebProxy != null)
            {
                WebProxy proxy = WebRequest.DefaultWebProxy as WebProxy;
                if (proxy.Address.AbsoluteUri != string.Empty)
                {
                    _Proxy = proxy;
                }
            }
        }

        public void SetNetworkCredential(string username, string password)
        {
            if ((username != null) && (password != null))
                _Credentials = new NetworkCredential(username, password);
        }

        public void SetRange(long from, long to)
        {
            if (from < to)
                _Range = new long[2] { from, to };
            else
                _Range = null;
        }

        /// <summary>
        /// Initialize an HttpWebRequest for the current uri
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="Method">"GET" or "POST"</param>
        /// <param name="formData">null or Dictionary<string, string></param>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateWebRequest(Uri uri, string Method, Dictionary<string, string> formData)
        {
            //Fix uri at GET mode
            if ((Method == "GET") && (formData != null) && (formData.Count > 0))
            {
                UriBuilder uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (string key in formData.Keys)
                {
                    query[key] = formData[key];
                }
                uriBuilder.Query = query.ToString();
                uri = uriBuilder.Uri;
            }

            HttpWebRequest hwReq = WebRequest.Create(uri) as HttpWebRequest;

            hwReq.Method = Method;

            if (_CachePolicy != null)
                hwReq.CachePolicy = _CachePolicy;

            if (_Proxy != null)
                hwReq.Proxy = _Proxy;

            if (_UserAgent != null)
                hwReq.UserAgent = _UserAgent;

            if (_Accept != null)
                hwReq.Accept = _Accept;

            hwReq.AllowAutoRedirect = _AllowAutoRedirect;
            hwReq.KeepAlive = _KeepAlive;
            hwReq.Timeout = _Timeout;

            if (_Range != null)
                hwReq.AddRange(_Range[0], _Range[1]);

            if (_Referer != null)
                hwReq.Referer = _Referer;

            // Add authentication to request
            if (_Credentials != null)
                hwReq.Credentials = _Credentials;

            if (_RequestHeaders != null)
            {
                foreach (string key in _RequestHeaders.Keys)
                {
                    hwReq.Headers.Add(key, _RequestHeaders[key]);
                }
            }

            if (_AllowCookie)
                hwReq.CookieContainer = new CookieContainer();

            if (Method == "POST")
            {
                hwReq.ContentType = "application/x-www-form-urlencoded";

                StringBuilder data = new StringBuilder();
                string ampersand = "";
                foreach (string key in formData.Keys)
                {
                    data.Append(ampersand).Append(key).Append("=").Append(HttpUtility.UrlEncode(formData[key]));
                    ampersand = "&";
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                // 設定寫入內容長度
                hwReq.ContentLength = byteData.Length;

                // 寫入 POST 參數
                using (Stream postStream = hwReq.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }
            }

            return hwReq;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Accept = null;

                _CachePolicy = null;

                _ContentType = null;

                _Cookies = null;

                _Credentials = null;

                _Proxy = null;

                _Range = null;

                _Referer = null;

                _RequestHeaders = null;

#if _EnableWorkMode1
                _ResponseHeaders = null;

                if (_ResponseStream != null)
                    _ResponseStream.Dispose();

                _ResponseStream = null;

                _ResponseText = null;

                _TextEncoding = null;
#endif

                _UserAgent = null;
            }
        }
    }
}