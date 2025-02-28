using System.IO;
using System.Net.Http;
using System.Text;

namespace STranslate.Util;

public class HttpUtil
{
    #region 公共方法-GetAsync

    public static async Task<string> GetAsync(string url, int timeout = 10)
    {
        return await GetAsync(url, CancellationToken.None, timeout).ConfigureAwait(false);
    }

    public static async Task<string> GetAsync(string url, CancellationToken token, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        var response = await client.GetAsync(url, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task<string> GetAsync(string url, Dictionary<string, string>? queryParams,
        CancellationToken token, Dictionary<string, string>? headers = null, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        if (queryParams is { Count: > 0 })
        {
            var uriBuilder = new UriBuilder(url) { Query = BuildQueryString(queryParams) };
            url = uriBuilder.ToString();
        }
        if (headers is { Count: > 0 })
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        var response = await client.GetAsync(url, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    #endregion

    #region 公共方法-PostAsync

    public static async Task<string> PostAsync(string url, string req, int timeout = 10)
    {
        return await PostAsync(url, req, CancellationToken.None, timeout).ConfigureAwait(false);
    }

    public static async Task<string> PostAsync(string url, string req, CancellationToken token, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        var response = await client.PostAsync(url, new StringContent(req, Encoding.UTF8, "application/json"), token)
            .ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task<string> PostAsync(string url, Tuple<string, string> req, CancellationToken token,
        int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        var content = new StringContent($"{req.Item1}={Uri.EscapeDataString(req.Item2)}", Encoding.UTF8,
            "application/x-www-form-urlencoded");
        var response = await client.PostAsync(url, content, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task<string> PostAsync(
        string url,
        string? req,
        Dictionary<string, string>? queryParams,
        Dictionary<string, string> headers,
        CancellationToken token = default,
        int timeout = 10
    )
    {
        using var client = CreateHttpClient(timeout);
        if (queryParams is { Count: > 0 })
        {
            var uriBuilder = new UriBuilder(url) { Query = BuildQueryString(queryParams) };
            url = uriBuilder.ToString();
        }

        using var request = CreateHttpRequestMessage(HttpMethod.Post, url, req, headers);
        var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task<string> PostAsync(
        string url,
        Dictionary<string, string[]>? headers,
        Dictionary<string, string[]>? parameters,
        CancellationToken token = default,
        int timeout = 10
    )
    {
        using var client = CreateHttpClient(timeout);
        HttpResponseMessage? response = null;

        if (parameters != null)
        {
            var content = new StringBuilder();
            foreach (var parameter in parameters.SelectMany(p =>
                         p.Value.Select(v => new KeyValuePair<string, string>(p.Key, v))))
            {
                if (content.Length > 0) content.Append('&');
                content.Append($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
            }

            var stringContent =
                new StringContent(content.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await client.PostAsync(url, stringContent, token).ConfigureAwait(false);
            return await GetResponseContentAsync(response, token).ConfigureAwait(false);
        }

        var emptyContent = new StringContent(string.Empty);
        if (headers != null)
            foreach (var header in headers)
                foreach (var value in header.Value)
                    client.DefaultRequestHeaders.Add(header.Key, value);

        response = await client.PostAsync(url, emptyContent, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    /// <summary>
    ///     Post-FromData
    /// </summary>
    /// <param name="url"></param>
    /// <param name="formData"></param>
    /// <param name="token"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> PostAsync(string url, Dictionary<string, string> formData, CancellationToken token = default,
        int timeout = 10)
    {
        return await PostAsync(url, formData, null, token, timeout).ConfigureAwait(false);
    }

    /// <summary>
    ///     Post-FromData
    ///     * 该方法支持设置Headers
    /// </summary>
    /// <param name="url"></param>
    /// <param name="formData"></param>
    /// <param name="headers"></param>
    /// <param name="token"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async Task<string> PostAsync(string url, Dictionary<string, string> formData, Dictionary<string, string>? headers = null, CancellationToken token = default, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(formData) };
        if (headers != null)
            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);
        var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task PostAsync(Uri uri, string req, string? key, Action<string> onDataReceived,
        CancellationToken token, int timeout = 10)
    {
        var header = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {key}" }
        };
        await PostAsync(uri, header, req, onDataReceived, token, timeout);
    }

    public static async Task PostAsync(Uri uri, Dictionary<string, string>? header, string req, Action<string> onDataReceived,
        CancellationToken token = default, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        { Content = new StringContent(req, Encoding.UTF8, "application/json") };

        if (header != null)
            foreach (var item in header)
            {
                request.Headers.Add(item.Key, item.Value);
            }

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
            .ConfigureAwait(false);
        await ResponseCheckAsync(response, token).ConfigureAwait(false);

        await using var responseStream = await response.Content.ReadAsStreamAsync(token);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !token.IsCancellationRequested)
        {
            var content = await reader.ReadLineAsync(token);
            if (!string.IsNullOrEmpty(content))
                onDataReceived?.Invoke(content);
        }
    }

    private static async Task ResponseCheckAsync(HttpResponseMessage response, CancellationToken token = default)
    {
        if (response.IsSuccessStatusCode)
            return;

        // 以ChatGLM为例: 外层错误作为分类，内层有更详细的描述
        // https://open.bigmodel.cn/dev/api#error-code-v4
        var outerMsg = response.ReasonPhrase;
        var innerMsg = await response.Content.ReadAsStringAsync(token);
        throw new Exception(outerMsg, new Exception(innerMsg));
    }

    #endregion

    #region 公共方法-DownloadFileAsync

    /// <summary>
    ///     下载文件并保存到指定路径
    /// </summary>
    /// <param name="url">文件下载地址</param>
    /// <param name="savePath">保存路径的目录</param>
    /// <param name="fileName">要保存的文件名</param>
    /// <param name="token">取消令牌</param>
    /// <param name="timeout">超时时间(秒)</param>
    /// <returns>返回保存的文件完整路径</returns>
    public static async Task<string?> DownloadFileAsync(string url, string savePath, string fileName, CancellationToken token = default, int timeout = 30)
    {
        string fullPath = Path.Combine(savePath, fileName);

        using var client = CreateHttpClient(timeout);
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        await ResponseCheckAsync(response, token).ConfigureAwait(false);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        await stream.CopyToAsync(fileStream, token).ConfigureAwait(false);

        return fullPath;
    }

    /// <summary>
    ///     下载文件并保存到指定路径
    /// </summary>
    /// <param name="url">文件下载地址</param>
    /// <param name="savePath">保存路径的目录</param>
    /// <param name="fileName">要保存的文件名</param>
    /// <param name="headers">请求头</param>
    /// <param name="token">取消令牌</param>
    /// <param name="timeout">超时时间(秒)</param>
    /// <returns>返回保存的文件完整路径</returns>
    public static async Task<string?> DownloadFileAsync(string url, string savePath, string fileName, Dictionary<string, string>? headers = null, CancellationToken token = default, int timeout = 30)
    {
        string fullPath = Path.Combine(savePath, fileName);

        using var client = CreateHttpClient(timeout);
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers != null)
            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        await ResponseCheckAsync(response, token).ConfigureAwait(false);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        await stream.CopyToAsync(fileStream, token).ConfigureAwait(false);

        return fullPath;
    }

    #endregion

    #region 私有方法

    private static HttpClient CreateHttpClient(int timeout)
    {
        var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
        return client;
    }

    private static async Task<string> GetResponseContentAsync(HttpResponseMessage response, CancellationToken token)
    {
        await ResponseCheckAsync(response, token).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
    }

    private static string BuildQueryString(Dictionary<string, string> queryParams)
    {
        return string.Join("&",
            queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, string? content,
        Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, new Uri(url));
        if (content != null) request.Content = new StringContent(content, Encoding.UTF8, "application/json");

        if (headers != null)
            foreach (var header in headers)
                if (header.Key == "Authorization")
                    request.Headers.TryAddWithoutValidation("Authorization", header.Value);
                else
                    request.Headers.Add(header.Key, header.Value);

        return request;
    }

    #endregion
}