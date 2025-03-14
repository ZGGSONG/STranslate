using System.IO;
using System.Net;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels;

namespace STranslate.Helper;

public class ExternalCallHelper
{
    private HttpListener? _listener;

    public bool IsStarted { get; private set; }

    /// <summary>
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="isStopFirst"></param>
    public void StartService(string prefix, bool isStopFirst = false)
    {
        if (isStopFirst)
            StopService();

        if (IsStarted)
            return;

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);

            _listener.Start();
            _listener.BeginGetContext(Callback, _listener);
            IsStarted = true;
        }
        catch (Exception ex)
        {
            const string msg = "启动服务失败请重新配置端口";
            LogService.Logger.Error($"{msg}: {prefix}", ex);
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip(msg);
        }
    }

    public void StopService()
    {
        if (!IsStarted)
            return;

        _listener?.Close();
        _listener = null;
        IsStarted = false;
    }

    private void Callback(IAsyncResult ar)
    {
        if (!IsStarted || _listener == null || !_listener.IsListening)
            return;

        HttpListenerContext context;
        try
        {
            context = _listener.EndGetContext(ar);
        }
        catch (Exception)
        {
            // HttpListener has been disposed, no need to handle the request
            return;
        }

        _listener.BeginGetContext(Callback, _listener);

        try
        {
            var request = context.Request;

            // Get the URL from the request
            var uri = request.Url ?? throw new Exception("get url is null");

            if (uri.Segments.Length > 2)
                throw new Exception("path does not meet the requirements");

            // Get the path from the URL
            var path = uri.AbsolutePath.TrimStart('/');
            path = path == "" ? "translate" : path;

            // Get the external call action based on the path
            var ecAction = GetExternalCallAction(path);

            // Read the content of the request
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var content = reader.ReadToEnd();

            //Please use GET like `curl localhost:50020/translate -d \"hello world\"`"
            switch (request.HttpMethod)
            {
                case "GET":
                    WeakReferenceMessenger.Default.Send(new ExternalCallMessenger(ecAction, ""));
                    break;

                case "POST":
                    WeakReferenceMessenger.Default.Send(new ExternalCallMessenger(ecAction, content));
                    break;

                default:
                    throw new Exception("Method Not Allowed");
            }

            ResponseHandler(context.Response);
        }
        catch (Exception e)
        {
            ResponseHandler(context.Response, e.Message);
            LogService.Logger.Error($"ExternalCall Error, {e.Message}", e);
        }
    }

    /// <summary>
    ///     字符串=>外部调用枚举
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private ExternalCallAction GetExternalCallAction(string source)
    {
        return Enum.TryParse<ExternalCallAction>(source, out var eAction)
            ? eAction
            : throw new Exception("path does not meet the requirements");
    }

    private void ResponseHandler(HttpListenerResponse response, string? error = null)
    {
        response.StatusCode = HttpStatusCode.OK.GetHashCode();
        response.ContentType = "application/json;charset=UTF-8";
        response.ContentEncoding = Encoding.UTF8;
        response.AppendHeader("Content-Type", "application/json;charset=UTF-8");

        var data = new
        {
            code = error is null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            data = error ?? "Call Succeed"
        };

        using StreamWriter writer = new(response.OutputStream, Encoding.UTF8);
        writer.Write(JsonConvert.SerializeObject(data));
        writer.Close();
        response.Close();
    }
}