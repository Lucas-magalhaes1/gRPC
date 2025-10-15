using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Tasks.Server.Infrastructure;

public class ServerLoggingInterceptor : Interceptor
{
    private readonly ILogger<ServerLoggingInterceptor> _logger;
    public ServerLoggingInterceptor(ILogger<ServerLoggingInterceptor> logger) => _logger = logger;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Unary {Method} from {Peer}", context.Method, context.Peer);
        try
        {
            var resp = await continuation(request, context);
            _logger.LogInformation("Unary {Method} OK", context.Method);
            return resp;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Unary {Method} RPC error {Status}", context.Method, ex.StatusCode);
            throw;
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("ServerStreaming {Method} from {Peer}", context.Method, context.Peer);
        await continuation(request, responseStream, context);
        _logger.LogInformation("ServerStreaming {Method} completed", context.Method);
    }
}