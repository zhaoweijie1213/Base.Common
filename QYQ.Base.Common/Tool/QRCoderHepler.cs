using QRCoder;
using System;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// 二维码服务类
    /// </summary>
    public class QRCoderHepler
    {
        #region 简单调用（保持原有签名）

        /// <summary>
        /// 生成 PNG 二维码字节流（默认配置）
        /// </summary>
        public static Task<byte[]> GenerateQRCodeAsync(string url)
        {
            // 兼容旧逻辑：默认像素 20
            return GenerateQRCodeAsync(url, qr => qr.GetGraphic(20));
        }

        /// <summary>
        /// 生成 Base64 PNG 二维码（默认配置）
        /// </summary>
        public static Task<string> GenerateBase64QRCodeAsync(string url)
        {
            // 兼容旧逻辑：默认像素 10
            return GenerateBase64QRCodeAsync(url, qr => qr.GetGraphic(20));
        }

        #endregion

        #region 高级调用（把 GetGraphic 逻辑传进来）

        /// <summary>
        /// 生成 PNG 二维码字节流，调用方可完全控制 GetGraphic 参数
        /// </summary>
        /// <param name="url">内容</param>
        /// <param name="getGraphic">
        ///  例如：qr => qr.GetGraphic(20)
        ///        qr => qr.GetGraphic(20, darkColorRgba, lightColorRgba, drawQuietZones: true)
        /// </param>
        public static Task<byte[]> GenerateQRCodeAsync(
            string url,
            Func<PngByteQRCode, byte[]> getGraphic)
        {
            QRCodeGenerator gen = new();
            QRCodeData data = gen.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);

            var png = getGraphic(new PngByteQRCode(data));
            return Task.FromResult(png);
        }

        /// <summary>
        /// 生成 Base64 PNG 二维码，调用方可完全控制 GetGraphic 参数
        /// </summary>
        /// <param name="url">内容</param>
        /// <param name="getGraphic">
        ///  例如：qr => qr.GetGraphic(20)
        ///        qr => qr.GetGraphic(20, darkColorRgba, lightColorRgba, drawQuietZones: true)
        /// </param>
        public static Task<string> GenerateBase64QRCodeAsync(
            string url,
            Func<Base64QRCode, string> getGraphic)
        {
            QRCodeGenerator gen = new();
            QRCodeData data = gen.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);

            var base64 = getGraphic(new Base64QRCode(data));
            return Task.FromResult($"data:image/png;base64,{base64}");
        }

        #endregion

    }
}
