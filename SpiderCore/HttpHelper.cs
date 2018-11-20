using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace SpiderCore
{
    /// <summary>
    /// Http连接操作帮助类
    /// </summary>
    public class HttpHelper
    {
        //默认的编码
        private Encoding _encoding = Encoding.Default;
        //Post数据编码
        private Encoding _postencoding = Encoding.Default;

        //HttpWebRequest对象用来发起请求
        private HttpWebRequest _request;

        //获取影响流的数据对象
        private HttpWebResponse _response;

        /// <summary>
        /// 根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="objhttpitem">参数类对象</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetHtml(HttpItem objhttpitem)
        {
            //返回参数
            var result = new HttpResult();
            try
            {
                //准备参数
                SetRequest(objhttpitem);
            }
            catch (Exception ex)
            {
                result = new HttpResult
                {
                    Cookie = string.Empty,
                    Header = null,
                    Html = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message
                };
                return result;
            }
            try
            {
                using (_response = (HttpWebResponse)_request.GetResponse())
                {
                    result.StatusCode = _response.StatusCode;
                    result.StatusDescription = _response.StatusDescription;
                    result.Header = _response.Headers;
                    if (_response.Cookies != null) result.CookieCollection = _response.Cookies;
                    if (_response.Headers["set-cookie"] != null) result.Cookie = _response.Headers["set-cookie"];
                    //GZIIP处理
                    MemoryStream stream = GetMemoryStream(_response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase) ?
                        // ReSharper disable AssignNullToNotNullAttribute
                                                              new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress) :
                        // ReSharper restore AssignNullToNotNullAttribute
                                                              _response.GetResponseStream());
                    //获取Byte
                    byte[] rawResponse = stream.ToArray();
                    stream.Close();
                    //是否返回Byte类型数据
                    if (objhttpitem.ResultType == ResultType.Byte) result.ResultByte = rawResponse;
                    //从这里开始我们要无视编码了
                    if (_encoding == null)
                    {
                        Match meta = Regex.Match(Encoding.Default.GetString(rawResponse), "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                        string charter = (meta.Groups.Count > 1) ? meta.Groups[2].Value.ToLower() : string.Empty;
                        if (charter.Length > 2)
                            _encoding = Encoding.GetEncoding(charter.Trim().Replace("\"", "").Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk"));
                        else
                        {
                            _encoding = string.IsNullOrEmpty(_response.CharacterSet) ? Encoding.UTF8 :
                                Encoding.GetEncoding(_response.CharacterSet);
                        }
                    }
                    //得到返回的HTML
                    result.Html = _encoding.GetString(rawResponse);
                }
            }
            catch (WebException ex)
            {
                //这里是在发生异常时返回的错误信息
                _response = (HttpWebResponse)ex.Response;
                result.Html = ex.Message;
                if (_response != null)
                {
                    result.StatusCode = _response.StatusCode;
                    result.StatusDescription = _response.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            if (objhttpitem.IsToLower) result.Html = result.Html.ToLower();
            return result;
        }

        /// <summary>
        /// 4.0以下.net版本取数据使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            var stream = new MemoryStream();
            const int length = 256;
            var buffer = new Byte[length];
            int bytesRead = streamResponse.Read(buffer, 0, length);
            while (bytesRead > 0)
            {
                stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, length);
            }
            return stream;
        }

        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="objhttpItem">参数列表</param>
        private void SetRequest(HttpItem objhttpItem)
        {
            // 验证证书
            SetCer(objhttpItem);
            //设置Header参数
            if (objhttpItem.Header != null && objhttpItem.Header.Count > 0) foreach (string item in objhttpItem.Header.AllKeys)
                {
                    _request.Headers.Add(item, objhttpItem.Header[item]);
                }
            // 设置代理
            SetProxy(objhttpItem);
            if (objhttpItem.ProtocolVersion != null) _request.ProtocolVersion = objhttpItem.ProtocolVersion;
            _request.ServicePoint.Expect100Continue = objhttpItem.Expect100Continue;
            //请求方式Get或者Post
            _request.Method = objhttpItem.Method;
            _request.Timeout = objhttpItem.Timeout;
            _request.ReadWriteTimeout = objhttpItem.ReadWriteTimeout;
            //Accept
            _request.Accept = objhttpItem.Accept;
            //ContentType返回类型
            _request.ContentType = objhttpItem.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            _request.UserAgent = objhttpItem.UserAgent;
            // 编码
            _encoding = objhttpItem.Encoding;
            //设置Cookie
            SetCookie(objhttpItem);
            //来源地址
            _request.Referer = objhttpItem.Referer;
            //是否执行跳转功能
            _request.AllowAutoRedirect = objhttpItem.Allowautoredirect;
            //设置Post数据
            SetPostData(objhttpItem);
            //设置最大连接
            if (objhttpItem.Connectionlimit > 0) _request.ServicePoint.ConnectionLimit = objhttpItem.Connectionlimit;
        }

        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCer(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest)WebRequest.Create(objhttpItem.URL);
                SetCerList(objhttpItem);
                //将证书添加到请求里
                _request.ClientCertificates.Add(new X509Certificate(objhttpItem.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest)WebRequest.Create(objhttpItem.URL);
                SetCerList(objhttpItem);
            }
        }

        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCerList(HttpItem objhttpItem)
        {
            if (objhttpItem.ClentCertificates == null || objhttpItem.ClentCertificates.Count <= 0) return;
            foreach (X509Certificate item in objhttpItem.ClentCertificates)
            {
                _request.ClientCertificates.Add(item);
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetCookie(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.Cookie))
                //Cookie
                _request.Headers[HttpRequestHeader.Cookie] = objhttpItem.Cookie;
            //设置Cookie
            if (objhttpItem.CookieCollection == null) return;
            _request.CookieContainer = new CookieContainer();
            _request.CookieContainer.Add(objhttpItem.CookieCollection);
        }

        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetPostData(HttpItem objhttpItem)
        {
            //验证在得到结果时是否有传入数据
            if (!_request.Method.Trim().ToLower().Contains("post")) return;
            if (objhttpItem.PostEncoding != null)
            {
                _postencoding = objhttpItem.PostEncoding;
            }
            byte[] buffer = null;
            //写入Byte类型
            if (objhttpItem.PostDataType == PostDataType.Byte && objhttpItem.PostdataByte != null && objhttpItem.PostdataByte.Length > 0)
            {
                //验证在得到结果时是否有传入数据
                buffer = objhttpItem.PostdataByte;
            }//写入文件
            else if (objhttpItem.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(objhttpItem.Postdata))
            {
                var r = new StreamReader(objhttpItem.Postdata, _postencoding);
                buffer = _postencoding.GetBytes(r.ReadToEnd());
                r.Close();
            } //写入字符串
            else if (!string.IsNullOrEmpty(objhttpItem.Postdata))
            {
                buffer = _postencoding.GetBytes(objhttpItem.Postdata);
            }
            if (buffer != null)
            {
                _request.ContentLength = buffer.Length;
                _request.GetRequestStream().Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="objhttpItem">参数对象</param>
        private void SetProxy(HttpItem objhttpItem)
        {
            if (string.IsNullOrEmpty(objhttpItem.ProxyIp)) return;
            //设置代理服务器
            if (objhttpItem.ProxyIp.Contains(":"))
            {
                string[] plist = objhttpItem.ProxyIp.Split(':');
                var myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()))
                {
                    Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd)
                };
                //建议连接
                //给当前请求对象
                _request.Proxy = myProxy;
            }
            else
            {
                var myProxy = new WebProxy(objhttpItem.ProxyIp, false)
                {
                    Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd)
                };
                //建议连接
                //给当前请求对象
                _request.Proxy = myProxy;
            }
            //设置安全凭证
            _request.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }

    }

    /// <summary>
    /// Http请求参考类
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string URL { get; set; }

        string _method = "GET";
        /// <summary>
        /// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值
        /// </summary>
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        int _timeout = 100000;
        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        int _readWriteTimeout = 30000;
        /// <summary>
        /// 默认写入Post数据超时间
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _readWriteTimeout; }
            set { _readWriteTimeout = value; }
        }

        string _accept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept
        {
            get { return _accept; }
            set { _accept = value; }
        }

        string _contentType = "text/html";
        /// <summary>
        /// 请求返回类型默认 text/html
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        string _userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        /// <summary>
        /// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding { get; set; }

        private PostDataType _postDataType = PostDataType.String;
        /// <summary>
        /// Post的数据类型
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _postDataType; }
            set { _postDataType = value; }
        }

        string _postdata = string.Empty;
        /// <summary>
        /// Post请求时要发送的字符串Post数据
        /// </summary>
        public string Postdata
        {
            get { return _postdata; }
            set { _postdata = value; }
        }

        /// <summary>
        /// Post请求时要发送的Byte类型的Post数据
        /// </summary>
        public byte[] PostdataByte { get; set; }

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        string _cookie = string.Empty;
        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        string _referer = string.Empty;
        /// <summary>
        /// 来源地址，上次访问地址
        /// </summary>
        public string Referer
        {
            get { return _referer; }
            set { _referer = value; }
        }

        string _cerPath = string.Empty;
        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath
        {
            get { return _cerPath; }
            set { _cerPath = value; }
        }

        /// <summary>
        /// 是否设置为全文小写，默认为不转化
        /// </summary>
        public bool IsToLower { get; set; }

        /// <summary>
        /// 支持跳转页面，查询结果将是跳转后的页面，默认是不跳转
        /// </summary>
        public bool Allowautoredirect { get; set; }

        private int _connectionlimit = 1024;
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int Connectionlimit
        {
            get { return _connectionlimit; }
            set { _connectionlimit = value; }
        }

        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; }

        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd { get; set; }

        /// <summary>
        /// 代理 服务IP
        /// </summary>
        public string ProxyIp { get; set; }

        private ResultType _resulttype = ResultType.String;
        /// <summary>
        /// 设置返回类型String和Byte
        /// </summary>
        public ResultType ResultType
        {
            get { return _resulttype; }
            set { _resulttype = value; }
        }

        private WebHeaderCollection _header = new WebHeaderCollection();
        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header
        {
            get { return _header; }
            set { _header = value; }
        }

        /// <summary>
        /// 获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。
        /// </summary>
        public Version ProtocolVersion { get; set; }

        private Boolean _expect100Continue = true;
        /// <summary>
        ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public Boolean Expect100Continue
        {
            get { return _expect100Continue; }
            set { _expect100Continue = value; }
        }

        /// <summary>
        /// 设置509证书集合
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }

        public HttpItem()
        {
            URL = string.Empty;
            Allowautoredirect = false;
            IsToLower = false;
            CookieCollection = null;
            Encoding = null;
        }

        /// <summary>
        /// 设置或获取Post参数编码,默认的为Default编码
        /// </summary>
        public Encoding PostEncoding { get; set; }
    }

    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte { get; set; }

        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }

        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
    }

    /// <summary>
    /// 返回类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 表示只返回字符串 只有Html有数据
        /// </summary>
        String,

        /// <summary>
        /// 表示返回字符串和字节流 ResultByte和Html都有数据返回
        /// </summary>
        Byte
    }

    /// <summary>
    /// Post的数据格式默认为string
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// 字符串类型，这时编码Encoding可不设置
        /// </summary>
        String,

        /// <summary>
        /// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空
        /// </summary>
        Byte,

        /// <summary>
        /// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值
        /// </summary>
        FilePath
    }

}
