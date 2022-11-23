using Bloc.Memory;
using Bloc.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bloc.Values
{
    public sealed class Task : Value
    {
        internal Task(Func<Value> func) => Value = System.Threading.Tasks.Task.Run(func);

        internal Task<Value> Value { get; }

        internal override ValueType GetType() => ValueType.Task;

        public override bool Equals(Value other)
        {
            if (other is not Task task)
                return false;

            if (Value != task.Value)
                return false;

            return true;
        }

        internal static Task Construct(List<Value> values, Call call)
        {
            return values.Count switch
            {
                0 => new(() => Void.Value),
                1 => values[0] switch
                {
                    Null => new(() => Void.Value),
                    Func func => new(() => func.Invoke(new(), call)),
                    Task task => task,
                    _ => throw new Throw($"'task' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'task' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString(int _)
        {
            return "[task]";
        }

        internal Value Await()
        {
            try
            {
                return Value.Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}