using System.Security.Cryptography;
using System.Text;

namespace STranslate.Util;

public static class YoudaoAuthV3Util
{
    /*
        添加鉴权相关参数 -
        appKey : 应用ID
        salt : 随机值
        curtime : 当前时间戳(秒)
        signType : 签名版本
        sign : 请求签名

        @param appKey    您的应用ID
        @param appSecret 您的应用密钥
        @param paramsMap 请求参数表
    */

    public static void AddAuthParams(string appKey, string appSecret, Dictionary<string, string[]> paramsMap)
    {
        var q = "";
        string[] qArray;
        if (paramsMap.ContainsKey("q"))
            qArray = paramsMap["q"];
        else
            qArray = paramsMap["img"];
        foreach (var item in qArray) q += item;
        var salt = Guid.NewGuid().ToString();
        var curtime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
        var sign = CalculateSign(appKey, appSecret, q, salt, curtime);
        paramsMap.Add("appKey", [appKey]);
        paramsMap.Add("salt", [salt]);
        paramsMap.Add("curtime", [curtime]);
        paramsMap.Add("signType", ["v3"]);
        paramsMap.Add("sign", [sign]);
    }

    /*
        计算鉴权签名 -
        计算方式 : sign = sha256(appKey + input(q) + salt + curtime + appSecret)

        @param appKey    您的应用ID
        @param appSecret 您的应用密钥
        @param q         请求内容
        @param salt      随机值
        @param curtime   当前时间戳(秒)
        @return 鉴权签名sign
    */

    public static string CalculateSign(string appKey, string appSecret, string q, string salt, string curtime)
    {
        var strSrc = appKey + GetInput(q) + salt + curtime + appSecret;
        return Encrypt(strSrc);
    }

    private static string Encrypt(string strSrc)
    {
        var inputBytes = Encoding.UTF8.GetBytes(strSrc);
        var hashedBytes = SHA256.HashData(inputBytes);
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpperInvariant();
    }

    private static string GetInput(string q)
    {
        if (q == null) return "";
        var len = q.Length;
        return len <= 20 ? q : q[..10] + len + q.Substring(len - 10, 10);
    }
}