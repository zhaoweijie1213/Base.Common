# CryptoToolkit

> 通用加密解密算法库，支持 .NET Framework、.NET Core 2.1~9.0

[![NuGet](https://zhaoweijie-oss.oss-cn-chengdu.aliyuncs.com/img/CryptoToolkit.svg)](https://www.nuget.org/packages/QYQ.CryptoToolkit/)

## 功能特性

- ✅ 支持 AES-CBC 加密解密
- ✅ 支持 AES-GCM 加密解密
- ✅ 支持 RSA 非对称加密解密
- ✅ 支持 MD5 / HMAC-SHA256 摘要算法
- ✅ HEX 字符串编码与解码
- ✅ 多目标框架兼容 `.NET Framework` + `.NET Core 2.1 - .NET 9.0`
- ✅ 完善的 XML 注释，IntelliSense 智能提示支持
- ✅ 可通过 GitHub Actions 自动发布 NuGet 包

## 安装方式

```shell
dotnet add package CryptoToolkit
```



## 使用示例

```csharp
using CryptoToolkit.Symmetric;

// AES-CBC 加密
string cipherText = AesCbcEncryptor.Encrypt(\"Hello World\", base64Key, base64IV);
string plainText = AesCbcEncryptor.Decrypt(cipherText, base64Key, base64IV);
```