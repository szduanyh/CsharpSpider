using System.Text;

namespace SpiderCore
{
    /// <summary>
    /// http处理基类
    /// 抽象类
    /// 封装http访问参数对象和帮助对象
    /// 通过继承，实现方便、可扩展和可维护的具体访问类
    /// </summary>
    public class BaseControl
    {
        /// <summary>
        /// 访问实体
        /// </summary>
        protected HttpItem CurrentHttpItem { get; set; }

        /// <summary>
        /// 访问执行
        /// </summary>
        protected HttpHelper CurrentHttpHelper { get; set; }

        /// <summary>
        /// 设置访问实体
        /// </summary>
        /// <param name="encoding">编码</param>
        /// <param name="method">Get | Post</param>
        /// <param name="userAgent">默认</param>
        /// <param name="accept">默认</param>
        /// <param name="contentType">内容类型：text/html</param>
        /// <param name="resultType">返回值类型：ResultType.String</param>
        /// <param name="allowautoredirect">允许重定向</param>
        /// <param name="cookieString">cookie</param>
        public virtual void SetHttpItem(
            Encoding encoding = null, string method = "GET",
            string userAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.22 Safari/537.36",
            string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            string contentType = "text/html", ResultType resultType = ResultType.String,
            bool allowautoredirect = false, string cookieString = "")
        {
            CurrentHttpItem.ContentType = contentType;
            CurrentHttpItem.Method = method;
            CurrentHttpItem.UserAgent = userAgent;
            CurrentHttpItem.Accept = accept;
            CurrentHttpItem.Encoding = encoding ?? Encoding.UTF8;
            CurrentHttpItem.ResultType = resultType;
            CurrentHttpItem.Allowautoredirect = allowautoredirect;
            CurrentHttpItem.Cookie = cookieString;
        }

        /// <summary>
        /// 设置访问url
        /// </summary>
        /// <param name="url"></param>
        public virtual void SetUrl(string url)
        {
            CurrentHttpItem.URL = url;
        }

        /// <summary>
        /// 根据设置抓取网页源码
        /// </summary>
        /// <returns></returns>
        protected virtual string GetHtml()
        {
            return CurrentHttpHelper.GetHtml(CurrentHttpItem).Html;
        }
    }
}
