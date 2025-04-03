using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common
{
    /// <summary>
    /// 全局公共对象
    /// </summary>
    public class GobalCommonObject
    {
        /// <summary>
        /// windows密钥文件夹
        /// </summary>
        public const string WindowsKeyFolder = @"C:\Users\Default\AppData\Local\Data\config";

        /// <summary>
        /// linux密钥文件夹
        /// </summary>
        public const string LinuxKeyFolder = "/etc/ssl/private";


        /// <summary>
        /// 获取路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsKeyFolder;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxKeyFolder;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}
