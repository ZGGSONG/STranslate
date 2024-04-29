using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using STranslate.Log;
using STranslate.Model;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace STranslate.Helper;

public class ExternalCallHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="isStopFirst"></param>
    public void StartService(string prefix, bool isStopFirst = false)
    {
        if (isStopFirst) StopService();

        if (_isStarted) return;

        _listener ??= new HttpListener();
        _listener.Prefixes.Clear();
        _listener.Prefixes.Add(prefix);

        _listener.Start();
        _listener.BeginGetContext(Callback, _listener);
        _isStarted = true;
    }

    public void StopService()
    {
        if (!_isStarted) return;

        _listener?.Close();
        _listener = null;
        _isStarted = false;
    }

    private void GetHandler(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var content = reader.ReadToEnd();

        var uri = request.Url ?? throw new Exception("get url is null");
        var path = uri.AbsolutePath.TrimStart('/');
        var ecAction = GetExternalCallAction(path);

        //弃用qurey形式传参
        //var collection = HttpUtility.ParseQueryString(uri.Query);
        //string queryKey = "screenshot";
        //var internalScreenshot = true;
        //if (collection.AllKeys.Contains(queryKey))
        //{
        //    internalScreenshot = !bool.TryParse(collection[queryKey], out internalScreenshot) || internalScreenshot;
        //}
        WeakReferenceMessenger.Default.Send(new ExternalCallMessenger(ecAction, content));
    }

    /// <summary>
    /// 字符串=>外部调用枚举
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private ExternalCallAction GetExternalCallAction(string source) => Enum.TryParse<ExternalCallAction>(source, out var eAction) ? eAction : ExternalCallAction.translate;

    private void Callback(IAsyncResult ar)
    {
        if (!_isStarted || _listener == null || !_listener.IsListening)
            return;

        var context = _listener.EndGetContext(ar);
        _listener.BeginGetContext(Callback, _listener);

        try
        {
            var request = context.Request;

            // 仅接受 GET 请求
            if (request.HttpMethod == "GET")
                GetHandler(request);
            else
                throw new Exception("Please use GET like `curl 127.0.0.1:50020/translate -d 'helloworld'`");

            ResponseHandler(context.Response);
        }
        catch (Exception e)
        {
            ResponseHandler(context.Response, e.Message);
            LogService.Logger.Error($"ExternalCall Error, {e.Message}", e);
        }
    }

    private void ResponseHandler(HttpListenerResponse response, string? error = null)
    {
        response.StatusCode = HttpStatusCode.OK.GetHashCode();
        response.ContentType = "application/json;charset=UTF-8";
        response.ContentEncoding = Encoding.UTF8;
        response.AppendHeader("Content-Type", "application/json;charset=UTF-8");

        var data = new { code = error is null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, data = error ?? "Call Succeed" };

        using StreamWriter writer = new(response.OutputStream, Encoding.UTF8);
        writer.Write(JsonConvert.SerializeObject(data));
        writer.Close();
        response.Close();
    }

    private HttpListener? _listener;

    private bool _isStarted;

    public bool IsStarted => _isStarted;
}
