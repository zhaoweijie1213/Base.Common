using QRCoder;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// 二维码服务类
    /// </summary>
    public class QRCoderHepler
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<byte[]> GenerateQRCodeAsync(string url)
        {
            return await Task.Run(() =>
            {
                var png = GeneratePng(url, pr => pr.GetGraphic(20));
                return png;
            });
        }

        /// <summary>
        /// 生成base64二维码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GenerateBase64QRCodeAsync(string url)
        {
            return await Task.Run(() =>
            {
                var png = GenerateBase64QRCode(url, pr => pr.GetGraphic(10));
                return $"data:image/png;base64,{png}";
            });
        }

        /// <summary>
        /// 生成Image
        /// </summary>
        /// <param name="content"></param>
        /// <param name="getGraphic"></param>
        /// <returns></returns>
        public static Image<Rgba32> GenerateImage(string content, Func<QRCode, Image<Rgba32>> getGraphic)
        {
            QRCodeGenerator gen = new();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.H);
            return getGraphic(new QRCode(data));
        }

        /// <summary>
        /// 生成PNG
        /// </summary>
        /// <param name="content"></param>
        /// <param name="getGraphic"></param>
        /// <returns></returns>
        public static byte[] GeneratePng(string content, Func<PngByteQRCode, byte[]> getGraphic)
        {
            QRCodeGenerator gen = new();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);
            return getGraphic(new PngByteQRCode(data));
        }

        /// <summary>
        /// 生成PNG
        /// </summary>
        /// <param name="content"></param>
        /// <param name="getGraphic"></param>
        /// <returns></returns>
        public static string GenerateBase64QRCode(string content, Func<Base64QRCode, string> getGraphic)
        {
            QRCodeGenerator gen = new();
            QRCodeData data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);
            return getGraphic(new Base64QRCode(data));
        }

    }
}
