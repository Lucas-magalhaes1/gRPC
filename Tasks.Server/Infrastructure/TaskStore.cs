using System.Collections.Concurrent;
using System.Linq;
using Proto = Tasks.Proto;

namespace Tasks.Server.Infrastructure;

public class TaskStore
{
    private readonly ConcurrentDictionary<string, Proto.Task> _db = new();

    public Proto.Task Add(Proto.Task t)
    {
        _db[t.Id] = t;
        return t;
    }

    public bool TryGet(string id, out Proto.Task t) => _db.TryGetValue(id, out t!);

    public IEnumerable<Proto.Task> List(Proto.Status status, int? limit, int? offset)
    {
        var q = _db.Values.AsEnumerable();

        //  compara sempre com o enum do proto
        if (!Equals(status, default(Proto.Status)))
            q = q.Where(task => task.Status == status);

        if (offset.HasValue && offset.Value > 0) q = q.Skip(offset.Value);
        if (limit.HasValue  && limit.Value  > 0) q = q.Take(limit.Value);

        return q;
    }

    public Proto.Task Update(Proto.Task updated)
    {
        _db[updated.Id] = updated;
        return updated;
    }
}