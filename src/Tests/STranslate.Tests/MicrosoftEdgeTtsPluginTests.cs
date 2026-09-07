using STranslate.Plugin;
using System.Net;
using System.Reflection;
using System.Text.Json;
using EdgeMain = STranslate.Plugin.Tts.MicrosoftEdge.Main;
using EdgeSettings = STranslate.Plugin.Tts.MicrosoftEdge.Settings;

namespace STranslate.Tests;

public class MicrosoftEdgeTtsPluginTests
{
    private const string ExpectedOrigin = "https://tts.wangwangit.com";

    [Theory]
    [InlineData("https://tts.wangwangit.com/v1/audio/speech")]
    [InlineData("https://TTS.WANGWANGIT.COM/v1/audio/speech")]
    [InlineData("https://tts.wangwangit.com:443/v1/audio/speech")]
    public async Task PlayAudioAsync_TargetHttpsEndpoint_AddsOnlyExpectedOrigin(string url)
    {
        var harness = CreateHarness(new EdgeSettings { Url = url });

        await harness.Plugin.PlayAudioAsync("你好");

        var call = Assert.Single(harness.Http.Calls);
        var headers = Assert.IsType<Dictionary<string, string>>(call.Options?.Headers);
        var header = Assert.Single(headers);
        Assert.Equal("Origin", header.Key);
        Assert.Equal(ExpectedOrigin, header.Value);
    }

    [Theory]
    [InlineData("https://example.com/v1/audio/speech")]
    [InlineData("https://tts.wangwangit.com.example.com/v1/audio/speech")]
    [InlineData("https://prefix-tts.wangwangit.com/v1/audio/speech")]
    [InlineData("http://tts.wangwangit.com/v1/audio/speech")]
    [InlineData("https://tts.wangwangit.com:444/v1/audio/speech")]
    [InlineData("/v1/audio/speech")]
    [InlineData("not a valid URL")]
    public async Task PlayAudioAsync_OtherUrl_ForwardsWithoutOrigin(string url)
    {
        var harness = CreateHarness(new EdgeSettings { Url = url });

        await harness.Plugin.PlayAudioAsync("你好");

        var call = Assert.Single(harness.Http.Calls);
        Assert.Equal(url, call.Url);
        AssertNoOrigin(call.Options);
    }

    [Fact]
    public async Task PlayAudioAsync_UrlChanges_CreatesIndependentRequestOptionsWithoutReusingOrigin()
    {
        var settings = new EdgeSettings();
        var harness = CreateHarness(settings);

        await harness.Plugin.PlayAudioAsync("first");
        await harness.Plugin.PlayAudioAsync("second");
        settings.Url = "https://example.com/v1/audio/speech";
        await harness.Plugin.PlayAudioAsync("third");

        Assert.Equal(3, harness.Http.Calls.Count);
        var firstOptions = Assert.IsType<Options>(harness.Http.Calls[0].Options);
        var secondOptions = Assert.IsType<Options>(harness.Http.Calls[1].Options);
        Assert.NotSame(firstOptions, secondOptions);
        Assert.Equal(ExpectedOrigin, Assert.IsType<Dictionary<string, string>>(firstOptions.Headers)["Origin"]);
        Assert.Equal(ExpectedOrigin, Assert.IsType<Dictionary<string, string>>(secondOptions.Headers)["Origin"]);
        AssertNoOrigin(harness.Http.Calls[2].Options);
    }

    [Fact]
    public async Task PlayAudioAsync_Success_ForwardsPayloadAudioAndCancellationToken()
    {
        byte[] response = [0x49, 0x44, 0x33, 0x04];
        var settings = new EdgeSettings
        {
            Voice = "en-US-JennyNeural",
            Speed = 1.25,
            Pitch = -7,
            Style = "cheerful"
        };
        var harness = CreateHarness(settings, (_, _) => Task.FromResult(response));
        using var cancellation = new CancellationTokenSource();

        await harness.Plugin.PlayAudioAsync("hello", cancellation.Token);

        var call = Assert.Single(harness.Http.Calls);
        Assert.Equal(cancellation.Token, call.CancellationToken);
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(call.Content));
        var properties = payload.RootElement.EnumerateObject().ToArray();
        Assert.Equal(["input", "voice", "speed", "pitch", "style"], properties.Select(property => property.Name));
        Assert.Equal(JsonValueKind.String, properties[0].Value.ValueKind);
        Assert.Equal("hello", properties[0].Value.GetString());
        Assert.Equal(JsonValueKind.String, properties[1].Value.ValueKind);
        Assert.Equal("en-US-JennyNeural", properties[1].Value.GetString());
        Assert.Equal(JsonValueKind.Number, properties[2].Value.ValueKind);
        Assert.Equal(1.25, properties[2].Value.GetDouble());
        Assert.Equal(JsonValueKind.String, properties[3].Value.ValueKind);
        Assert.Equal("-7", properties[3].Value.GetString());
        Assert.Equal(JsonValueKind.String, properties[4].Value.ValueKind);
        Assert.Equal("cheerful", properties[4].Value.GetString());

        var playback = Assert.Single(harness.Audio.Calls);
        Assert.Same(response, playback.AudioData);
        Assert.Equal(cancellation.Token, playback.CancellationToken);
    }

    [Fact]
    public async Task PlayAudioAsync_HttpFailure_PropagatesExceptionWithoutPlaybackOrRetry()
    {
        var expected = new HttpRequestException("TTS unavailable", null, HttpStatusCode.Forbidden);
        var harness = CreateHarness(
            new EdgeSettings(),
            (_, _) => Task.FromException<byte[]>(expected));

        var actual = await Assert.ThrowsAsync<HttpRequestException>(() => harness.Plugin.PlayAudioAsync("hello"));

        Assert.Same(expected, actual);
        Assert.Single(harness.Http.Calls);
        Assert.Empty(harness.Audio.Calls);
    }

    [Fact]
    public async Task PlayAudioAsync_HttpCancellation_PropagatesCancellationWithoutPlaybackOrRetry()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var harness = CreateHarness(
            new EdgeSettings(),
            (_, token) => Task.FromCanceled<byte[]>(token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => harness.Plugin.PlayAudioAsync("hello", cancellation.Token));

        var call = Assert.Single(harness.Http.Calls);
        Assert.Equal(cancellation.Token, call.CancellationToken);
        Assert.Empty(harness.Audio.Calls);
    }

    private static TestHarness CreateHarness(
        EdgeSettings settings,
        Func<int, CancellationToken, Task<byte[]>>? postBehavior = null)
    {
        var http = new HttpRecorder(postBehavior);
        var audio = new AudioRecorder();
        var context = CreateProxy<IPluginContext>((method, _) => method.Name switch
        {
            "get_HttpService" => http.Proxy,
            "get_AudioPlayer" => audio.Proxy,
            "LoadSettingStorage" when method.IsGenericMethod
                && method.GetGenericArguments()[0] == typeof(EdgeSettings) => settings,
            "Dispose" => null,
            _ => throw new NotSupportedException($"Unexpected IPluginContext call: {method}")
        });
        var plugin = new EdgeMain();
        plugin.Init(context);
        return new TestHarness(plugin, http, audio);
    }

    private static void AssertNoOrigin(Options? options)
    {
        Assert.True(options?.Headers is null || !options.Headers.ContainsKey("Origin"));
    }

    private static T CreateProxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, DelegateDispatchProxy>();
        ((DelegateDispatchProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    private sealed record TestHarness(EdgeMain Plugin, HttpRecorder Http, AudioRecorder Audio);

    private sealed record HttpCall(
        string Url,
        object Content,
        Options? Options,
        CancellationToken CancellationToken);

    private sealed record AudioCall(byte[] AudioData, CancellationToken CancellationToken);

    private sealed class HttpRecorder
    {
        private readonly Func<int, CancellationToken, Task<byte[]>> _postBehavior;

        public HttpRecorder(Func<int, CancellationToken, Task<byte[]>>? postBehavior)
        {
            _postBehavior = postBehavior ?? ((_, _) => Task.FromResult<byte[]>([0x01]));
            Proxy = CreateProxy<IHttpService>(Invoke);
        }

        public IHttpService Proxy { get; }

        public List<HttpCall> Calls { get; } = [];

        private object? Invoke(MethodInfo method, object?[]? arguments)
        {
            if (method.Name != nameof(IHttpService.PostAsBytesAsync)
                || arguments is not { Length: 4 }
                || arguments[0] is not string url
                || arguments[1] is not object content
                || arguments[3] is not CancellationToken cancellationToken)
            {
                throw new NotSupportedException($"Unexpected IHttpService call: {method}");
            }

            Calls.Add(new HttpCall(url, content, arguments[2] as Options, cancellationToken));
            return _postBehavior(Calls.Count, cancellationToken);
        }
    }

    private sealed class AudioRecorder
    {
        public AudioRecorder()
        {
            Proxy = CreateProxy<IAudioPlayer>(Invoke);
        }

        public IAudioPlayer Proxy { get; }

        public List<AudioCall> Calls { get; } = [];

        private object? Invoke(MethodInfo method, object?[]? arguments)
        {
            if (method.Name == nameof(IAudioPlayer.PlayAsync)
                && arguments is [byte[] audioData, CancellationToken cancellationToken])
            {
                Calls.Add(new AudioCall(audioData, cancellationToken));
                return Task.CompletedTask;
            }

            if (method.Name == nameof(IDisposable.Dispose))
            {
                return null;
            }

            throw new NotSupportedException($"Unexpected IAudioPlayer call: {method}");
        }
    }

    public class DelegateDispatchProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return Handler(targetMethod!, args);
        }
    }
}
