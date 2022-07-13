using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values
{
    public class Type : Value, IInvokable
    {
        public Type() => Value = new();

        public Type(ValueType type) => Value = new() { type };

        public Type(HashSet<ValueType> types) => Value = types;

        //public static Type None { get; } = new(new HashSet<ValueType>());
        //public static Type Any { get; } = new(Enum.GetValues(typeof(ValueType)).Cast<ValueType>().ToHashSet());

        public HashSet<ValueType> Value { get; }

        public override ValueType GetType() => ValueType.Type;

        public override bool Equals(Value other)
        {
            if (other is Type type)
                return Value.SetEquals(type.Value);

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Type))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast type as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Type => this,
                _ => throw new Throw($"Cannot cast type as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            //if (Value.Count == 0)
            //    return "[empty type]";

            //return string.Join(" | ", Value).ToLower();

            return "[type]";
        }

        public Value Invoke(List<Value> _0, Call _1)
        {
            if (Value.Count != 1)
                throw new Throw("Cannot instantiate a composite type");

            return Value.Single() switch
            {
                ValueType.Void => Void.Value,
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.False,
                ValueType.Number => new Number(),
                ValueType.Range => new Range(),
                ValueType.String => String.Empty,
                ValueType.Array => new Array(),
                ValueType.Struct => new Struct(),
                ValueType.Tuple => new Tuple(),
                ValueType.Function => new Function(),
                ValueType.Task => new Task(),
                ValueType.Reference => new Reference(),
                ValueType.Complex => new Complex(),
                ValueType.Type => new Type(),
                _ => throw new Exception()
            };
        }
    }
}