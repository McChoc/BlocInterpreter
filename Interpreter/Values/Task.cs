using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Task : Value
    {
        private readonly Task<Value> _task;

        internal Task(Func<Value> func)
        {
            _task = System.Threading.Tasks.Task.Run(func);
        }

        internal override ValueType GetType() => ValueType.Task;

        internal static Task Construct(List<Value> values, Call call)
        {
            return values.Count switch
            {
                0 => new(() => Void.Value),
                1 => values[0] switch
                {
                    Null => new(() => Void.Value),
                    Func func => new(() => func.Execute(call)),
                    Task task => task,
                    _ => throw new Throw($"'task' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'task' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString()
        {
            return "[task]";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_task);
        }

        public override bool Equals(object other)
        {
            return other is Task task &&
                _task == task._task;
        }

        internal Value Await()
        {
            try
            {
                return _task.Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}