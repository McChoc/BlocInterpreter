using System.Threading.Tasks;
using Bloc.Results;

namespace Bloc.Values
{
    public class Task : Value
    {
        internal Task() : this(() => Void.Value) { }

        internal Task(System.Func<Value> func) => Value = System.Threading.Tasks.Task.Run(func);

        internal Task<Value> Value { get; }

        public override ValueType GetType() => ValueType.Task;

        public override bool Equals(Value other)
        {
            if (other is not Task task)
                return false;

            if (Value != task.Value)
                return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Task))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast task as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Task => this,
                _ => throw new Throw($"Cannot cast task as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return "[task]";
        }
    }
}