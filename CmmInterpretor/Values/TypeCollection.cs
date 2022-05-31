using CmmInterpretor.Interfaces;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class TypeCollection : Value, IInvokable
    {
        public static TypeCollection None { get; } = new(new HashSet<ValueType>());
        public static TypeCollection Any { get; } = new(System.Enum.GetValues(typeof(ValueType)).Cast<ValueType>().ToHashSet());

        public HashSet<ValueType> Value { get; }

        public override ValueType Type => ValueType.Type;

        public TypeCollection(ValueType type) => Value = new HashSet<ValueType>() { type };
        public TypeCollection(HashSet<ValueType> types) => Value = types;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is TypeCollection t)
                return Value.SetEquals(t.Value);

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(TypeCollection))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast type as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Type => this,
                _ => throw new Throw($"Cannot cast type as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            if (Value.Count == 0)
                return "type()";

            return string.Join(" | ", Value).ToLower();
        }

        public IValue Invoke(List<Value> _0, Call _1)
        {
            if (Value.Count != 1)
                throw new Throw("Cannot instantiate a composite type");

            return Value.Single() switch
            {
                ValueType.Void => Void.Value,
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.False,
                ValueType.Number => new Number(0),
                ValueType.Range => new Range(null, null),
                ValueType.String => String.Empty,
                ValueType.Array => Array.Empty,
                ValueType.Struct => Struct.Empty,
                ValueType.Tuple => new Tuple(new()),
                ValueType.Function => new Function(),
                ValueType.Task => new Task(),
                ValueType.Reference => new Reference(null),
                ValueType.Complex => new Complex(null),
                ValueType.Type => None,
                _ => throw new System.Exception()
            };
        }
    }
}
