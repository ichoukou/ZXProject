using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Peacock.Common.Helper
{
    public class FileStreamHelper
    {
        /// <summary>
        /// 获取网站根目录
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationRootDir()
        {
            return System.Web.HttpRuntime.BinDirectory.Substring(0, System.Web.HttpRuntime.BinDirectory.TrimEnd('\\').LastIndexOf("\\")) + "\\";
        }

        /// <summary>
        /// 获得目录的物理路径
        /// </summary>
        /// <param name="sPath">文件路径</param>
        /// <returns></returns>
        public static string GetPathAtApplication(string sPath)
        {
            string result = "";
            if (sPath.StartsWith("~"))
            {
                result = sPath.Replace("~/", GetApplicationRootDir()).Replace("/", "\\");
            }
            else
            {
                result = GetApplicationRootDir() + sPath;
            }
            result = result.Replace("/", "\\").Replace("\\\\", "\\");
            return result;
        }

        /// <summary>
        /// 判断文件夹是否存在，不存在则新建
        /// </summary>
        /// <param name="sFolderPath">文件夹路径</param>
        public static void IsExistsDirectory(string sFolderPath)
        {
            if (!Directory.Exists(sFolderPath))
            {
                Directory.CreateDirectory(sFolderPath);
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <returns></returns>
        public static bool IsExits(string sFilePath)
        {
            bool result = File.Exists(sFilePath);
            return result;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sSourceFilePath">源文件路径</param>
        /// <param name="sTargetFilePath">目标文件路径</param>
        public static void CopyFile(string sSourceFilePath, string sTargetFilePath)
        {
            if (IsExits(sSourceFilePath))
            {
                File.Copy(sSourceFilePath, sTargetFilePath, true);
            }
        }

        /// <summary>
        /// 获取文件的扩展名
        /// </summary>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        public static string FileFormat(string sFileName)
        {
            string result = string.Empty;
            result = Path.GetExtension(sFileName).Replace(".", "");
            return result;
        }

        /// <summary>
        /// 用于MVC文件下载
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContentType(string sFileName)
        {
            string contentType = "application/octet-stream"; //"application/x-zip-compressed";
            string ext = Path.GetExtension(sFileName).ToLower();
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
            {
                contentType = registryKey.GetValue("Content Type").ToString();
            }
            return contentType;

        }

        /// <summary>
        /// 检测文件是否存在
        /// </summary>
        /// <param name="fullFilePath">文件完成路径</param>
        /// <returns></returns>
        public static bool CheckFileExeit(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将 Stream 转成 byte[]
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 将 byte[] 转成 Stream
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        /// <summary>
        /// 获取临时文件夹的存放地址
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            string result = string.Empty;
            string Path = ConfigurationManager.AppSettings["TempFilePath"];
            string Year = DateTime.Now.ToString("yyyy");
            string Month = DateTime.Now.ToString("MM");
            string Day = DateTime.Now.ToString("dd");
            result = string.Format(@"{0}\{1}\{2}\{3}\", Path, Year, Month, Day);
            return result;
        }

        /// <summary>
        /// 获取临时文件存放的物理路径
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string GetTempFilePath(string fileName)
        {
            string result = string.Empty;
            string TempPath = GetTempFilePath();
            result = UrlConvertorLocal(TempPath);
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            result += fileName;
            return result;
        }

        /// <summary>
        /// 获取上传文件夹的存放地址
        /// </summary>
        /// <returns></returns>
        public static string GetUploadFilePath()
        {
            string result = string.Empty;
            string Path = ConfigurationManager.AppSettings["UploadFilePath"];
            string Year = DateTime.Now.ToString("yyyy");
            string Month = DateTime.Now.ToString("MM");
            string Day = DateTime.Now.ToString("dd");
            result = string.Format(@"{0}\{1}\{2}\{3}\", Path, Year, Month, Day);
            return result;
        }

        /// <summary>
        /// 获取上传文件存放的物理路径
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string GetUploadFilePath(string fileName)
        {
            string result = string.Empty;
            string TempPath = GetUploadFilePath();
            result = UrlConvertorLocal(TempPath);
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            result += fileName;
            return result;
        }

        /// <summary>
        /// 本地路径转换成URL相对路径
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string LocalConvertorUrl(string Path)
        {

            //string tmpRootDir = System.Web.HttpContext.Current.Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath.ToString());//获取程序根目录
            string tmpRootDir = HttpRuntime.AppDomainAppPath.ToString();
            string imagesurl2 = Path.Replace(tmpRootDir, ""); //转换成相对路径
            imagesurl2 = imagesurl2.Replace(@"\", @"/");
            return imagesurl2;
        }

        /// <summary>
        /// 相对路径转换成服务器本地物理路径
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string UrlConvertorLocal(string Path)
        {
            //string tmpRootDir = System.Web.HttpContext.Current.Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath.ToString());//获取程序根目录
            string tmpRootDir = HttpRuntime.AppDomainAppPath.ToString();
            string imagesurl2 = tmpRootDir + Path.Replace(@"/", @"\"); //转换成绝对路径
            return imagesurl2;
        }
    }
}
