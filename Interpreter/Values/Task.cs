using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Task : Value
    {
        private readonly Func<Value> _func;
        private readonly Task<Value> _task;

        internal Task(Func<Value> func)
        {
            _func = func;
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
                    Func func => new(() => func.Invoke(new(), new(), call)),
                    Task task => task,
                    _ => throw new Throw($"'task' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'task' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override Value Copy()
        {
            return new Task(_func);
        }

        internal override string ToString(int _)
        {
            return "[task]";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_task);
        }

        public override bool Equals(object other) // TODO fix task copy and equality
        {
            if (other is not Task task)
                return false;

            if (_func != task._func)
                return false;

            return true;
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