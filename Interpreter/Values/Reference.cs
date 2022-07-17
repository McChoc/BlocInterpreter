using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Values
{
    public class Reference : Value
    {
        internal Reference() => Pointer = new VariablePointer(null);

        internal Reference(Pointer pointer) => Pointer = pointer;

        internal Pointer Pointer { get; }

        public override ValueType GetType() => ValueType.Reference;

        public override bool Equals(Value other)
        {
            if (other is not Reference reference)
                return false;

            return Pointer.Equals(reference.Pointer);
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Reference))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast reference as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Reference => this,
                _ => throw new Throw($"Cannot cast reference as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return "[reference]";
        }
    }
}
