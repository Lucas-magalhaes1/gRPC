using System;
using System.Threading.Tasks;
using Grpc.Core;                 // RpcException, StatusCode, CallOptions
using Grpc.Net.Client;
using Proto = Tasks.Proto;       // tipos gerados do .proto
using PStatus = Tasks.Proto.Status; // alias só para o enum Status do proto

class Program
{
    static async Task Main()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:50051");
        var client  = new Proto.TaskService.TaskServiceClient(channel);

        // Deadline/timeout de 5s em todas as chamadas deste demo
        CallOptions WithDeadline() => new(deadline: DateTime.UtcNow.AddSeconds(5));

        try
        {
            // 1) CreateTask (unária)
            var created = await client.CreateTaskAsync(
                new Proto.CreateTaskRequest { Titulo = "Estudar gRPC", Descricao = "Ler docs e implementar demo" },
                WithDeadline());
            Console.WriteLine($"Criada: {created.Task.Id} - {created.Task.Titulo}");

            // 2) GetTask (unária)
            var got = await client.GetTaskAsync(
                new Proto.GetTaskRequest { Id = created.Task.Id },
                WithDeadline());
            Console.WriteLine($"Get: {got.Task.Id} - {got.Task.Titulo} [{got.Task.Status}]");

            // 3) UpdateTask (unária)
            var updated = await client.UpdateTaskAsync(
                new Proto.UpdateTaskRequest
                {
                    Id = created.Task.Id,
                    Status = PStatus.Concluida   // <- use o alias do enum do proto
                },
                WithDeadline());
            Console.WriteLine($"Update: {updated.Task.Id} -> {updated.Task.Status}");

            // 4) ListTasks (server-streaming)
            using var call = client.ListTasks(
                new Proto.ListTasksRequest { Status = default, Limite = 10 },
                WithDeadline());

            await foreach (var task in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Stream item: {task.Id} | {task.Titulo} | {task.Status}");
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            Console.WriteLine("Erro: timeout (deadline exceeded)");
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"RPC error: {ex.StatusCode} - {ex.Status.Detail}");
        }
    }
}
