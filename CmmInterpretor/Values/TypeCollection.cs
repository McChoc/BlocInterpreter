using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class TypeCollection : Value, IInvokable
    {
        public static TypeCollection None { get; } = new(new HashSet<VariableType>());
        public static TypeCollection Any { get; } = new(System.Enum.GetValues(typeof(VariableType)).Cast<VariableType>().ToHashSet());

        public HashSet<VariableType> Value { get; }

        public override VariableType Type => VariableType.Type;

        public TypeCollection(VariableType type) => Value = new HashSet<VariableType>() { type };
        public TypeCollection(HashSet<VariableType> types) => Value = types;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is TypeCollection t)
                return Value.SetEquals(t.Value);

            return false;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            if (typeof(T) == typeof(TypeCollection))
            {
                value = this as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Type => this,
                _ => new Throw($"Cannot implicitly cast type as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Type => this,
                _ => new Throw($"Cannot cast type as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            if (Value.Count == 0)
                return "type()";
            else if (Value.Count == System.Enum.GetValues(typeof(VariableType)).Length)
                return "any";
            else
                return string.Join(" | ", Value).ToLower();
        }

        public IResult Invoke(List<Value> values, Engine engine)
        {
            if (Value.Count != 1)
                return new Throw("Cannot instantiate a composite type");

            return Value.Single() switch
            {
                VariableType.Void => Void.Value,
                VariableType.Null => Null.Value,
                VariableType.Bool => Bool.False,
                VariableType.Number => new Number(0),
                VariableType.Range => new Range(null, null),
                VariableType.String => String.Empty,
                VariableType.Array => Array.Empty,
                VariableType.Struct => Struct.Empty,
                VariableType.Tuple => new Tuple(new()),
                VariableType.Function => new Function(),
                VariableType.Task => new Task(),
                VariableType.Reference => new Reference(null),
                VariableType.Complex => new Complex(null),
                VariableType.Type => None,
                _ => throw new System.Exception()
            };
        }
    }
}
