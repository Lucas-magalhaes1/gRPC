using Grpc.Core;                       // RpcException, IServerStreamWriter, StatusCode
using RpcStatus = Grpc.Core.Status;    // alias para a classe de status do gRPC
using Proto = Tasks.Proto;             // tipos gerados do .proto
using STT = System.Threading.Tasks;    // Task de async/await
using Tasks.Server.Infrastructure;

namespace Tasks.Server;

public class TaskServiceImpl : Proto.TaskService.TaskServiceBase
{
    private readonly TaskStore _store;
    private readonly ILogger<TaskServiceImpl> _logger;

    public TaskServiceImpl(TaskStore store, ILogger<TaskServiceImpl> logger)
    {
        _store = store;
        _logger = logger;
    }

    public override STT.Task<Proto.CreateTaskResponse> CreateTask(
        Proto.CreateTaskRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            throw new RpcException(new RpcStatus(StatusCode.InvalidArgument, "Titulo obrigatório"));

        var t = new Proto.Task
        {
            Id          = Guid.NewGuid().ToString("N"),
            Titulo      = request.Titulo,
            Descricao   = request.Descricao ?? "",
            Status      = Proto.Status.Pendente,
            DataCriacao = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        _store.Add(t);
        _logger.LogInformation("Criada task {Id}", t.Id);

        return STT.Task.FromResult(new Proto.CreateTaskResponse { Task = t });
    }

    public override STT.Task<Proto.GetTaskResponse> GetTask(
        Proto.GetTaskRequest request, ServerCallContext context)
    {
        if (!_store.TryGet(request.Id, out var t))
            throw new RpcException(new RpcStatus(StatusCode.NotFound, "Task não encontrada"));

        return STT.Task.FromResult(new Proto.GetTaskResponse { Task = t });
    }

    public override STT.Task<Proto.UpdateTaskResponse> UpdateTask(
        Proto.UpdateTaskRequest request, ServerCallContext context)
    {
        if (!_store.TryGet(request.Id, out var current))
            throw new RpcException(new RpcStatus(StatusCode.NotFound, "Task não encontrada"));

        var updated = new Proto.Task
        {
            Id          = current.Id,
            Titulo      = string.IsNullOrWhiteSpace(request.Titulo) ? current.Titulo : request.Titulo,
            Descricao   = string.IsNullOrWhiteSpace(request.Descricao) ? current.Descricao : request.Descricao,
            Status = Equals(request.Status, default(Proto.Status)) ? current.Status : request.Status,
            DataCriacao = current.DataCriacao
        };

        _store.Update(updated);
        return STT.Task.FromResult(new Proto.UpdateTaskResponse { Task = updated });
    }

    public override async STT.Task ListTasks(
        Proto.ListTasksRequest request,
        IServerStreamWriter<Proto.Task> responseStream,
        ServerCallContext context)
    {
        var items = _store.List(
            request.Status,
            request.Limite > 0 ? request.Limite : null,
            request.Offset > 0 ? request.Offset : null);

        foreach (var t in items)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await responseStream.WriteAsync(t);
            await STT.Task.Delay(5, context.CancellationToken); // só p/ demonstrar streaming
        }
    }
}
