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
        CancellationToken token, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        if (queryParams is { Count: > 0 })
        {
            var uriBuilder = new UriBuilder(url) { Query = BuildQueryString(queryParams) };
            url = uriBuilder.ToString();
        }

        var response = await client.GetAsync(url, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    #endregion 公共方法-GetAsync

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
        CancellationToken token,
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
        CancellationToken token,
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
    public static async Task<string> PostAsync(string url, Dictionary<string, string> formData, CancellationToken token,
        int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(formData) };
        var response = await client.SendAsync(request, token).ConfigureAwait(false);
        return await GetResponseContentAsync(response, token).ConfigureAwait(false);
    }

    public static async Task PostAsync(Uri uri, string req, string? key, Action<string> onDataReceived,
        CancellationToken token, int timeout = 10)
    {
        using var client = CreateHttpClient(timeout);

        var request = new HttpRequestMessage(HttpMethod.Post, uri)
            { Content = new StringContent(req, Encoding.UTF8, "application/json") };

        if (!string.IsNullOrEmpty(key)) request.Headers.Add("Authorization", $"Bearer {key}");

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

    private static async Task ResponseCheckAsync(HttpResponseMessage response, CancellationToken token)
    {
        if (response.IsSuccessStatusCode)
            return;

        // 以ChatGLM为例: 外层错误作为分类，内层有更详细的描述
        // https://open.bigmodel.cn/dev/api#error-code-v4
        var outerMsg = response.ReasonPhrase;
        var innerMsg = await response.Content.ReadAsStringAsync(token);
        throw new Exception(outerMsg, new Exception(innerMsg));
    }

    #endregion 公共方法-PostAsync

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

    #endregion 私有方法
}