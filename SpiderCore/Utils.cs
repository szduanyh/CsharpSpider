using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SpiderCore
{
    public class Utils
    {
        public static bool ReqIeProxy { get; set; }

        public static HttpResult GetPageByUrl(string reqUrl, Dictionary<string, string> headers = null, Dictionary<string, string> postData = null, Encoding encoding = null)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = reqUrl,
                Method = "GET",
                Timeout = 100000, // 连接超时时间 ms
                ReadWriteTimeout = 30000, // 写入Post数据超时时间
                IsToLower = false, // 得到的HTML代码是否转成小写,可选项默认转小写
                Cookie = "", // 字符串
                UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.22 Safari/537.36",
                Accept = "text/html, application/xhtml+xml, */*",
                ContentType = "text/html", //返回类型
                Allowautoredirect = true, // 是否根据301跳转
                MaximumAutomaticRedirections = 10
                //ProxyIp = "192.168.1.105", // 代理服务器ID
                //ProxyPwd = "123456", // 代理服务器密码
                //ProxyUserName = "administrator", // 代理服务器账户名
                //ResultType = ResultType.String, // 返回数据类型，是Byte还是String
            };

            // 请求头
            if (headers != null && headers.Count > 0)
            {
                foreach (var key in headers.Keys)
                {
                    if (key.ToLower() == "referer")
                    {
                        item.Referer = headers[key];
                    }
                    else if (key.ToLower() == "cookie")
                    {
                        item.Cookie = headers[key];
                    }
                    else
                    {
                        item.Header.Add(key, headers[key]);
                    }
                }
            }

            if (postData != null)
            {
                item.Method = "POST";
                item.Postdata = string.Join("&", postData.Select(d => Uri.EscapeDataString(d.Key) + "=" + Uri.EscapeDataString(d.Value)));
                item.PostDataType = PostDataType.String;
                item.ContentType = "application/x-www-form-urlencoded";
            }

            if (encoding != null)
                item.Encoding = encoding; // 例如 Encoding.GetEncoding("gb2312");
            else
                item.Encoding = Encoding.GetEncoding("UTF-8");

            // 是否使用IE代理
            if (ReqIeProxy)
                item.ProxyIp = "ieproxy";

            HttpResult result = http.GetHtml(item);
            return result;
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="url">图片URL</param>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        public static void DownloadImgByUrl(string url, string dirPath, string fileName)
        {
            Dictionary<string, string> extLookup = new Dictionary<string, string>()
            {
                {"image/jpeg", "jpg"}, {"image/webp", "webp"}, {"image/gif", "gif"},
                {"image/png", "png"}, {"image/bmp", "bmp"}, {"image/x-icon", "ico"},
                {"image/tiff", "tif"}, {"image/svg+xml", "svg"}, {"image/x-xbitmap", "xbm"}
            };

            using (WebClient wc = new WebClient())
            {
                // 是否使用IE代理
                if (!ReqIeProxy)
                    wc.Proxy = null;

                byte[] fileBytes = wc.DownloadData(url);
                string fileType = wc.ResponseHeaders[HttpResponseHeader.ContentType];

                if (fileType != null && extLookup.ContainsKey(fileType))
                {
                    string ext = extLookup[fileType];
                    File.WriteAllBytes(Path.Combine(dirPath, string.Format("{0}.{1}", fileName, ext)), fileBytes);
                }
            }
        }

        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }


        /// <summary>
        /// Base 64 编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64Encode(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}
