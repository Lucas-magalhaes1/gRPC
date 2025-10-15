
# Sistema de Gerenciamento de Tarefas com gRPC

Este projeto implementa um sistema de gerenciamento de tarefas (to-do list) usando gRPC e Protocol Buffers. O sistema permite a criação, consulta, atualização e listagem de tarefas com suporte a streaming de dados.

## Estrutura do Projeto

Este repositório contém duas partes principais:

- **Servidor gRPC (Tasks.Server)**: Responsável por gerenciar as tarefas.
- **Cliente gRPC (Tasks.Client)**: Responsável por se conectar ao servidor e testar os métodos disponíveis.

## Arquitetura do Sistema

O sistema segue a arquitetura cliente-servidor, onde o servidor expõe um serviço gRPC, e o cliente consome esse serviço. O serviço gRPC é responsável por realizar operações de gerenciamento de tarefas, como:

- Criação de tarefa
- Consulta de tarefa
- Atualização de tarefa
- Listagem de tarefas (streaming)

## Tecnologias Utilizadas

- **gRPC**: Protocolo de comunicação de alta performance que utiliza HTTP/2.
- **Protocol Buffers (protobuf)**: Formato binário para serialização de dados.
- **C# (.NET 8)**: Framework para desenvolvimento do servidor e cliente gRPC.
- **ILogger**: Para registrar logs de execução do servidor.

## Estrutura de Pastas

- **Tasks.Server**: Contém a implementação do servidor gRPC.
- **Tasks.Client**: Contém o cliente que se conecta ao servidor para testar os métodos gRPC.

## Como Rodar o Projeto

### Pré-requisitos

- **.NET 8 SDK ou superior** instalado.
- **gRPC** e **Protocol Buffers** configurados corretamente.

### Passos para Execução

1. Clone o repositório:

    ```bash
    git clone https://github.com/seu-repositorio/grpc-tasks.git
    cd grpc-tasks
    ```

2. Restaure as dependências:

    Execute o comando abaixo para restaurar as dependências do projeto:

    ```bash
    dotnet restore
    ```

3. Construa o projeto:

    Em seguida, construa o projeto:

    ```bash
    dotnet build
    ```

4. Rode o Servidor:

    Para iniciar o servidor, execute o seguinte comando:

    ```bash
    dotnet run --project Tasks.Server
    ```

    O servidor estará rodando em **http://localhost:50051**.

5. Rode o Cliente:

    Em um outro terminal, execute o seguinte comando para rodar o cliente:

    ```bash
    dotnet run --project Tasks.Client
    ```

    O cliente irá fazer chamadas ao servidor para testar as operações de criação, consulta, atualização e listagem de tarefas.

## Métodos gRPC

O serviço gRPC oferece os seguintes métodos:

1. **CreateTask (unário)**

    Descrição: Cria uma nova tarefa.

    Request: `CreateTaskRequest` (título e descrição da tarefa).

    Response: `CreateTaskResponse` (tarefa criada com um ID gerado).

2. **GetTask (unário)**

    Descrição: Recupera uma tarefa existente pelo ID.

    Request: `GetTaskRequest` (ID da tarefa).

    Response: `GetTaskResponse` (tarefa encontrada).

3. **UpdateTask (unário)**

    Descrição: Atualiza os dados de uma tarefa (título, descrição ou status).

    Request: `UpdateTaskRequest` (ID da tarefa, título, descrição e status a serem atualizados).

    Response: `UpdateTaskResponse` (tarefa atualizada).

4. **ListTasks (streaming do servidor)**

    Descrição: Lista as tarefas com suporte a streaming.

    Request: `ListTasksRequest` (status opcional, limite e offset para paginação).

    Response: Resposta enviada em streaming, retornando várias tarefas.

### Exemplo de Uso

No cliente, a seguinte sequência de operações é executada:

**Criação de uma tarefa:**

O cliente cria uma nova tarefa com título e descrição. O servidor responde com a tarefa criada, incluindo um ID único.

```csharp
var created = await client.CreateTaskAsync(
    new Proto.CreateTaskRequest { Titulo = "Estudar gRPC", Descricao = "Ler docs e implementar demo" },
    WithDeadline());
```

**Consulta de uma tarefa:**

Após a criação, o cliente consulta a tarefa recém-criada utilizando o ID gerado.

```csharp
var got = await client.GetTaskAsync(new Proto.GetTaskRequest { Id = created.Task.Id }, WithDeadline());
```

**Atualização de uma tarefa:**

O cliente atualiza a tarefa para marcar o status como "Concluída".

```csharp
var updated = await client.UpdateTaskAsync(
    new Proto.UpdateTaskRequest { Id = created.Task.Id, Status = PStatus.Concluida },
    WithDeadline());
```

**Listagem de tarefas (streaming):**

O cliente lista todas as tarefas com um limite de 10 e sem filtro de status. O servidor responde com uma sequência de tarefas.

```csharp
using var call = client.ListTasks(new Proto.ListTasksRequest { Status = default, Limite = 10 }, WithDeadline());
await foreach (var task in call.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"Stream item: {task.Id} | {task.Titulo} | {task.Status}");
}
```

## Principais Desafios e Erros Resolvidos

1. **Ambiguidade entre Status do Protobuf e gRPC**

    Durante o desenvolvimento, encontramos um problema de ambiguidade de nome entre o enum `Status` do Protobuf (`Proto.Status`) e a classe `Status` do gRPC (`Grpc.Core.Status`). A solução foi usar aliases:

    ```csharp
    using PStatus = Tasks.Proto.Status;  // Alias para o enum Status
    ```

    E sempre usar `PStatus` ao invés de `Proto.Status` para o enum, e `Grpc.Core.Status` para o código de status gRPC.

2. **Problema com a Geração dos Arquivos Protobuf**

    Outro desafio foi garantir que os arquivos gerados a partir do .proto estivessem no diretório correto e fossem referenciados adequadamente no projeto. Isso foi resolvido configurando corretamente o `.csproj` para incluir os arquivos Protobuf com:

    ```xml
    <ItemGroup>
      <Protobuf Include="Protos	asks.proto" GrpcServices="Server" />
    </ItemGroup>
    ```

3. **Timeout (DeadlineExceeded)**

    Configurar timeouts para as chamadas gRPC foi uma das partes mais importantes. Para evitar que o cliente ficasse esperando indefinidamente, usamos o deadline de 5 segundos em todas as chamadas:

    ```csharp
    CallOptions WithDeadline() => new(deadline: DateTime.UtcNow.AddSeconds(5));
    ```

## Conclusão

Este projeto implementa um sistema de gerenciamento de tarefas utilizando gRPC com Protocol Buffers. O servidor gRPC foi configurado para gerenciar tarefas em memória, com suporte a unary RPCs e server-streaming. A comunicação entre cliente e servidor é rápida e eficiente graças ao uso de gRPC e Protocol Buffers.

