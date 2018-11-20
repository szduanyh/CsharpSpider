using System.Web;

namespace SpiderCore.AmazingBobble
{
    public class AmazingBobbleControl : BaseControl
    {
        private string UrlTemplate { get; set; }

        #region 构造区

        public AmazingBobbleControl(string urlTemplate)
        {
            UrlTemplate = urlTemplate;
            CurrentHttpItem = new HttpItem();
            CurrentHttpHelper = new HttpHelper();
            SetHttpItem();
        }

        #endregion

        #region 方法区

        private void SetUrl(string catename, int page = 1)
        {
            CurrentHttpItem.URL = string.Format(UrlTemplate, HttpUtility.UrlEncode(catename), page);
            CurrentHttpItem.ProxyIp = "104.248.61.157:8080";
        }

        public string GetFlightHtml(string catename, int page = 1)
        {
            SetUrl(catename, page);
            return CurrentHttpHelper.GetHtml(CurrentHttpItem).Html;
        }
        #endregion
    }
}
