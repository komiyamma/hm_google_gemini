using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;
using Grpc.Net.Client;
using System.Net;

internal partial class ChatSession
{
    AuthenticatedCallInvoker GetProxyAuthenticatedCallInvoker(string location, string proxy_url)
    {

        // プロキシの設定
        var proxyUri = proxy_url;

        var httpHandler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(proxyUri))
        {
            var proxy = new WebProxy(proxyUri)
            {
                UseDefaultCredentials = true // 必要に応じて認証を設定
            };
            httpHandler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = true
            };
        }
        else
        {
            httpHandler = new HttpClientHandler
            {
                UseProxy = false // プロキシなし通信を明示的に制御
            };
        }

        // 認証情報の取得
        var credential = Task.Run(() => GoogleCredential.GetApplicationDefaultAsync()).Result;
        var scopedCredential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");

        // GrpcChannel の作成
        var channel = GrpcChannel.ForAddress($"https://{location}-aiplatform.googleapis.com", new Grpc.Net.Client.GrpcChannelOptions
        {
            HttpHandler = httpHandler,
        });

        // CallInvoker に認証情報を適用
        AuthenticatedCallInvoker callInvoker = new AuthenticatedCallInvoker(channel.CreateCallInvoker(), scopedCredential);

        return callInvoker;

    }
}

class AuthenticatedCallInvoker : CallInvoker
{
    private readonly CallInvoker _baseInvoker;
    private readonly CallCredentials _callCredentials;

    public AuthenticatedCallInvoker(CallInvoker baseInvoker, GoogleCredential credential)
    {
        _baseInvoker = baseInvoker;
        _callCredentials = credential.ToCallCredentials();
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        string host,
        CallOptions options,
        TRequest request)
    {
        return _baseInvoker.AsyncUnaryCall(method, host, options.WithCredentials(_callCredentials), request);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        string host,
        CallOptions options,
        TRequest request)
    {
        return _baseInvoker.BlockingUnaryCall(method, host, options.WithCredentials(_callCredentials), request);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        string host,
        CallOptions options,
        TRequest request)
    {
        return _baseInvoker.AsyncServerStreamingCall(method, host, options.WithCredentials(_callCredentials), request);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        string host,
        CallOptions options)
    {
        return _baseInvoker.AsyncClientStreamingCall(method, host, options.WithCredentials(_callCredentials));
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        string host,
        CallOptions options)
    {
        return _baseInvoker.AsyncDuplexStreamingCall(method, host, options.WithCredentials(_callCredentials));
    }
}
