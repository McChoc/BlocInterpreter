using System.Collections.Generic;
using System.Threading.Tasks;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Task : Value
{
    private readonly Task<Value> _task;

    internal Task() : this(() => Void.Value) { }

    internal Task(System.Func<Value> func)
    {
        _task = System.Threading.Tasks.Task.Run(func);
    }

    public override ValueType GetType() => ValueType.Task;
    public override string ToString() => "[task]";

    internal Value Await()
    {
        try
        {
            return _task.Result;
        }
        catch (System.AggregateException e) when (e.InnerException is Throw t)
        {
            throw t;
        }
    }

    internal static Task Construct(List<Value> values, Call call)
    {
        return values switch
        {
            [] => new(),
            [Null] => new(),
            [Task task] => task,
            [Func func] => new(() => func.Execute(call)),
            [_] => throw new Throw($"'task' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'task' does not have a constructor that takes {values.Count} arguments")
        };
    }
}