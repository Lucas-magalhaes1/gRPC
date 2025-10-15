using Microsoft.AspNetCore.Server.Kestrel.Core;
using Tasks.Server;
using Tasks.Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Kestrel em HTTP/2 na porta 50051
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(50051, o =>
    {
        o.Protocols = HttpProtocols.Http2; // h2c (sem TLS)
    });
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerLoggingInterceptor>();
});

builder.Services.AddSingleton<TaskStore>(); 

var app = builder.Build();

app.MapGrpcService<TaskServiceImpl>();
app.MapGet("/", () => "gRPC Tasks.Server rodando em :50051");

app.Run();