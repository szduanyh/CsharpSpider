using HtmlAgilityPack;
using SpiderCore.AmazingBobble;

namespace CsharpSpider
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AmazingBobbleControl abControl = new AmazingBobbleControl("https://www.amazingbobbleheads.com/category/name/{0}?page={1}");

            string strHtml = abControl.GetFlightHtml("wedding", 1);


            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(strHtml);//加载HTML字符串，如果是文件可以用htmlDocument.Load方法加载

            HtmlNodeCollection cc = htmlDocument.DocumentNode.SelectNodes("//div[@class='mask']");
            foreach (HtmlNode htmlNode in cc)
            {
                string strHref = htmlNode.ParentNode.ParentNode.GetAttributeValue("href",string.Empty);
                if (string.IsNullOrEmpty(strHref) || "/category/name/custom".Equals(strHref))
                    continue;
                string strImg = htmlNode.ChildNodes["img"].GetAttributeValue("src",string.Empty);
                System.Console.WriteLine(strHref + "|" + strImg);
            }
            System.Console.WriteLine(cc.Count);
        }

        private static void SpiderContent()
        {
            string[] arrCate = { "wedding", "couples", "sports-and-hobbies", "musicians", "graduation", 
                                   "work", "casual-and-leisure", "vehicles", "pets","humorous"};

            for (int i = 0; i < arrCate.Length; i++)
            {
                SpiderByCate(arrCate[i]);
            }
        }

        private static void SpiderByCate(string _cateName)
        {
            for (int i = 1; i < 50; i++)
            {
                AmazingBobbleControl abControl = new AmazingBobbleControl("https://www.amazingbobbleheads.com/category/name/{0}?page={1}");
                string strHtml = abControl.GetFlightHtml(_cateName, i);

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(strHtml);//加载HTML字符串，如果是文件可以用htmlDocument.Load方法加载

                HtmlNodeCollection htmlCollection = htmlDocument.DocumentNode.SelectNodes("//div[@class='mask']");
                if (htmlCollection.Count <= 1)
                    break;

                foreach (HtmlNode htmlNode in htmlCollection)
                {
                    string strHref = htmlNode.ParentNode.ParentNode.GetAttributeValue("href", string.Empty);
                    if (string.IsNullOrEmpty(strHref) || "/category/name/custom".Equals(strHref))
                        continue;
                    string strImg = htmlNode.ChildNodes["img"].GetAttributeValue("src", string.Empty);
                }
                System.Console.WriteLine(htmlCollection.Count);
            }
        }
    }
}
