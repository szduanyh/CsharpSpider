using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpiderCore
{
    public class FileHelper
    {
        public static void ClearPath(string dstPath)
        {
            try
            {
                if (!new System.IO.DirectoryInfo(dstPath).Exists)
                    return;

                foreach (string d in System.IO.Directory.GetFileSystemEntries(dstPath))
                {
                    if (System.IO.File.Exists(d))
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(d);
                        DeleteFile(fi); // 直接删除其中的文件
                    }
                    else
                    {
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(d);
                        DeletePath(di.FullName); // 删除子文件夹   
                    }
                }
            }
            catch (Exception ex)
            {
                CLog.WriteLog("清除文件夹失败-{0},Message:{1},StackTrace:{2}", dstPath, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// 删除目录下的所有文件（只删除文件）
        /// </summary>
        /// <param name="dstPath"></param>
        public static void DeleteFiles(string dstPath)
        {
            try
            {
                if (!new System.IO.DirectoryInfo(dstPath).Exists)
                    return;

                foreach (string d in System.IO.Directory.GetFiles(dstPath))
                {
                    if (System.IO.File.Exists(d))
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(d);
                        DeleteFile(fi); // 直接删除其中的文件
                    }
                }
            }
            catch (Exception ex)
            {
                CLog.WriteLog("删除目录下的所有文件失败-{0},Message:{1},StackTrace:{2}", dstPath, ex.Message, ex.StackTrace);
            }
        }

        public static void DeletePath(string dstPath)
        {
            try
            {
                if (!Directory.Exists(dstPath))
                    return;

                foreach (string d in System.IO.Directory.GetFileSystemEntries(dstPath))
                {
                    if (System.IO.File.Exists(d))
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(d);
                        DeleteFile(fi); // 直接删除其中的文件
                    }
                    else
                        DeletePath(d); // 递归删除子文件夹   
                }

                System.IO.Directory.Delete(dstPath); // 删除已空文件夹
            }
            catch (Exception ex)
            {
                CLog.WriteLog("删除文件夹失败-{0},Message:{1},StackTrace:{2}", dstPath, ex.Message, ex.StackTrace);
            }
        }

        public static void CopyFile(string src, string dst)
        {
            try
            {
                if (src.ToLower() == dst.ToLower())
                    return;

                if (!File.Exists(src))
                    return;

                FileHelper.DeleteFile(dst);
                FileHelper.CreateDirectoy(Path.GetDirectoryName(dst));

                System.IO.File.Copy(src, dst);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("拷贝文件失败-{0}:{1},Message:{2},StackTrace:{3}", src, dst, ex.Message, ex.StackTrace);
            }
        }

        public static bool IsFileExist(string file)
        {
            return !string.IsNullOrEmpty(file) && File.Exists(file);
        }

        public static bool IsDirExist(string dir)
        {
            return !string.IsNullOrEmpty(dir) && Directory.Exists(dir);
        }

        public static void MoveFile(string src, string dst)
        {
            try
            {
                if (string.Equals(src, dst, StringComparison.CurrentCultureIgnoreCase))
                    return;

                if (!File.Exists(src))
                    return;

                FileHelper.DeleteFile(dst);
                FileHelper.CreateDirectoy(Path.GetDirectoryName(dst));

                System.IO.File.Move(src, dst);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("移动文件失败-{0}:{1},Message:{2},StackTrace:{3}", src, dst, ex.Message, ex.StackTrace);
            }
        }

        public static string DiskTypeInfo = "Disk";

        /// <summary>
        /// 移动整个文件夹
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="tgtDir"></param>
        public static void MoveDirectory(string srcDir, string tgtDir)
        {
            if (string.Equals(srcDir, tgtDir, StringComparison.CurrentCultureIgnoreCase))
                return;

            if (!Directory.Exists(srcDir))
                return;

            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            foreach (var file in files)
            {
                if (DiskTypeInfo == "Disk")
                {
                    File.Move(file.FullName, target.FullName + @"\" + file.Name);
                }
                else
                    File.Copy(file.FullName, target.FullName + @"\" + file.Name);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            foreach (var dir in dirs)
            {
                MoveDirectory(dir.FullName, target.FullName + @"\" + dir.Name);
            }
        }

        public static void CopyDirectory(string srcDir, string tgtDir)
        {
            if (srcDir.ToLower() == tgtDir.ToLower())
                return;

            if (!Directory.Exists(srcDir))
                return;

            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            foreach (var file in files)
            {
                File.Copy(file.FullName, target.FullName + @"\" + file.Name, true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            foreach (var dir in dirs)
            {
                CopyDirectory(dir.FullName, target.FullName + @"\" + dir.Name);
            }
        }

        public static void DeleteFile(string fileName)
        {
            try
            {
                FileInfo fi = new System.IO.FileInfo(fileName);
                DeleteFile(fi);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("删除文件失败-{0},Message:{1},StackTrace:{2}", fileName, ex.Message, ex.StackTrace);
            }
        }

        public static void DeleteFile(FileInfo fi)
        {
            try
            {
                if (fi.Exists)
                {
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = System.IO.FileAttributes.Normal;
                    System.IO.File.Delete(fi.FullName);

                    fi.Delete();
                }
            }
            catch (Exception ex)
            {
                CLog.WriteLog("删除文件失败-{0},Message:{1},StackTrace:{2}", fi.FullName, ex.Message, ex.StackTrace);
            }
        }

        public static void CreateDirectoyFromFileName(string fileName)
        {
            try
            {
                string path = Path.GetDirectoryName(fileName);
                if (!new System.IO.DirectoryInfo(path).Exists)
                    System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("创建文件夹失败-{0},Message:{1},StackTrace:{2}", fileName, ex.Message, ex.StackTrace);
            }
        }

        public static void CreateDirectoyByFileName(string fileName)
        {
            try
            {
                string dir = Path.GetDirectoryName(fileName);
                CreateDirectoy(dir);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("由文件创建文件夹失败-{0},Message:{1},StackTrace:{2}", fileName, ex.Message, ex.StackTrace);
            }
        }

        public static void CreateDirectoy(string dir)
        {
            try
            {
                if (!new System.IO.DirectoryInfo(dir).Exists)
                    System.IO.Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("创建文件夹失败-{0},Message:{1},StackTrace:{2}", dir, ex.Message, ex.StackTrace);
            }
        }

        public static string[] GetAllSubDirs(string root, SearchOption searchOption)
        {
            string[] all = null;
            try
            {
                all = Directory.GetDirectories(root, "*", searchOption);
            }
            catch
            {
                // ignored
            }

            return all;
        }

        public static List<string> GetAllPureSubDirs(string root)
        {
            List<string> list = new List<string>();
            try
            {
                string[] all = Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly);
                foreach (string path in all)
                {
                    string s = System.IO.Path.GetFileName(path);
                    list.Add(s);
                }
            }
            catch
            {
                // ignored
            }

            return list;
        }

        public static long GetFilesSize(string[] fileNames)
        {
            return fileNames.Sum(fileName => GetFileSize(fileName));
        }

        public static long GetFileSize(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                return new FileInfo(fileName).Length;

            return 0;
        }

        /// <summary>
        /// Files the content.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static byte[] FileContent(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);

                return buffur;
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取文件字节流失败: {0}", ex.Message);
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    //关闭资源
                    fs.Close();
                }
            }
        }

        public static bool SaveString2File(string content, string fileName)
        {
            StreamWriter sw = null;

            try
            {
                CreateDirectoyFromFileName(fileName);

                sw = new StreamWriter(fileName);
                sw.WriteLine(content);
            }
            catch (Exception ex)
            {
                CLog.WriteLog("保存内容错误-{0},Message:{1},StackTrace:{2}", fileName, ex.Message, ex.StackTrace);
                return false;
            }

            if (sw != null)
                sw.Close();

            return true;
        }

        public static string LoadStringFromFile(string fileName)
        {
            string content = string.Empty;

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName, System.Text.Encoding.UTF8);
                content = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                CLog.WriteLog("读取内容错误-{0},Message:{1},StackTrace:{2}", fileName, ex.Message, ex.StackTrace);
            }

            if (sr != null)
                sr.Close();

            return content;
        }

        /// <summary>
        /// 获取root文件夹下所有的文件,包含子文件夹中的文件
        /// </summary>
        /// <param name="root">文件夹路径</param>
        /// <param name="extension">文件扩展名,默认为所有文件</param>
        /// <returns>root文件夹下所有的文件的列表</returns>
        public static List<string> GetAllFileList(string root, string extension = "")
        {
            if (Directory.Exists(root))
            {
                var dirList = GetAllSubDirs(root, SearchOption.AllDirectories).ToList();
                dirList.Add(root);

                var list = new List<string>();

                foreach (var dir in dirList)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    dirInfo.GetFiles().ToList().ForEach(f =>
                    {
                        if (!string.IsNullOrWhiteSpace(extension))
                        {
                            if (f.Extension == (extension.Contains(".") ? extension : "." + extension))
                            {
                                list.Add(f.FullName);
                            }
                        }
                        else
                        {
                            list.Add(f.FullName);
                        }
                    });
                }

                return list;
            }
            else
            {
                return null;
            }
        }

        public class InlineComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> getEquals;
            private readonly Func<T, int> getHashCode;

            public InlineComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
            {
                this.getEquals = equals;
                this.getHashCode = hashCode;
            }

            public bool Equals(T x, T y)
            {
                return this.getEquals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return this.getHashCode(obj);
            }
        }
    }
}
