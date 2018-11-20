using System;
using System.IO;
using System.Text;

namespace SpiderCore
{
    public class CLog
    {
        /// <summary>
        /// 保存日志信息到文件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void WriteLog(string message, params object[] args)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string filename = DateTime.Now.ToString("yyyy-MM-dd");
            string filePath = root + "Log\\" + filename + ".txt";
            bool isTrue = CreateDirectory(root + "Log\\");//检查文件目录是否存在

            if (isTrue)
            {
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(filePath, true, Encoding.Default);
                    if (args.Length > 0)
                        message = string.Format(message, args);
                    sw.WriteLine(string.Format("{0} {1}\r\n", DateTime.Now.ToString("MM-dd HH:mm:ss:fff"), message));
                    sw.Flush();
                }
                catch { }
                finally { if (sw != null) sw.Close(); }
            }
        }

        /// <summary>
        /// 文件目录是否存在，不存在则新建
        /// </summary>
        /// <param name="path"> 决对路径,如:e:soft\log\</param>
        /// <returns></returns>
        private static bool CreateDirectory(string path)
        {
            string aa = Directory.GetDirectoryRoot(path);
            if (!Directory.Exists(Directory.GetDirectoryRoot(path)))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
