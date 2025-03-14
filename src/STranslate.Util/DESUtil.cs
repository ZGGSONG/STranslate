using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace STranslate.Util;

public class DESUtil
{
    // 默认密钥和初始化向量
    private const string DEFAULTKEY = "T*^%2>&.";

    private const string DEFAULTIV = "i*%&*n86";

    /// <summary>
    ///     使用DES算法进行加密
    /// </summary>
    /// <param name="data">要加密的数据</param>
    /// <param name="key">8位字符的密钥字符串</param>
    /// <param name="iv">8位字符的初始化向量字符串</param>
    /// <returns>加密后的Base64字符串</returns>
    public static string DesEncrypt(string data, string? key = null, string? iv = null)
    {
        // 将输入数据转换为ASCII编码的字节数组
        var bytes = Encoding.ASCII.GetBytes(data);

        // 将密钥和初始化向量转换为ASCII编码的字节数组，如果未提供则使用默认值
        var byKey = Encoding.ASCII.GetBytes(key ?? DEFAULTKEY);
        var byIV = Encoding.ASCII.GetBytes(iv ?? DEFAULTIV);

        // 创建DES加密算法实例
        using var des = DES.Create();
        // 创建内存流
        using var ms = new MemoryStream();
        // 创建加密流
        using var cs = new CryptoStream(ms, des.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
        // 写入加密后的数据到内存流
        cs.Write(bytes, 0, bytes.Length);
        // 刷新加密流的最终块
        cs.FlushFinalBlock();
        // 将内存流中的数据转换为Base64字符串并返回
        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    ///     使用DES算法进行解密
    /// </summary>
    /// <param name="data">要解密的Base64字符串</param>
    /// <param name="key">8位字符的密钥字符串（必须与加密时相同）</param>
    /// <param name="iv">8位字符的初始化向量字符串（必须与加密时相同）</param>
    /// <returns>解密后的字符串</returns>
    public static string DesDecrypt(string data, string? key = null, string? iv = null)
    {
        try
        {
            // 将Base64字符串转换为字节数组
            var bytes = Convert.FromBase64String(data);

            // 将密钥和初始化向量转换为ASCII编码的字节数组，如果未提供则使用默认值
            var byKey = Encoding.ASCII.GetBytes(key ?? DEFAULTKEY);
            var byIV = Encoding.ASCII.GetBytes(iv ?? DEFAULTIV);

            // 创建DES解密算法实例
            using var des = DES.Create();
            // 创建内存流
            using var ms = new MemoryStream();
            // 创建解密流
            using var cs = new CryptoStream(ms, des.CreateDecryptor(byKey, byIV), CryptoStreamMode.Write);
            // 写入解密后的数据到内存流
            cs.Write(bytes, 0, bytes.Length);
            // 刷新解密流的最终块
            cs.FlushFinalBlock();
            // 将内存流中的数据转换为ASCII编码的字符串并返回
            return Encoding.ASCII.GetString(ms.ToArray());
        }
        catch (Exception)
        {
            // 如果出现问题则返回传入值
            return data;
        }
    }
}