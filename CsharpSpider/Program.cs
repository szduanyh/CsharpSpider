using HtmlAgilityPack;
using SpiderCore.AmazingBobble;

namespace CsharpSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            AmazingBobbleControl abControl = new AmazingBobbleControl("https://www.amazingbobbleheads.com/category/name/{0}?page={1}");

            string strHtml = abControl.GetFlightHtml("wedding", 1);


            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(strHtml);//加载HTML字符串，如果是文件可以用htmlDocument.Load方法加载

            HtmlNodeCollection cc = htmlDocument.DocumentNode.SelectNodes("//div[@class='mask']");
            System.Console.WriteLine(cc.Count);
        }
    }
}
