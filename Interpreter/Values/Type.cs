using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Type : Value, IInvokable
    {
        public Type(ValueType type) => Value = new() { type };

        public Type(HashSet<ValueType> types) => Value = types;

        //public static Type None { get; } = new(new HashSet<ValueType>());
        //public static Type Any { get; } = new(Enum.GetValues(typeof(ValueType)).Cast<ValueType>().ToHashSet());

        public HashSet<ValueType> Value { get; }

        internal override ValueType GetType() => ValueType.Type;

        internal static Type Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(new HashSet<ValueType>()),
                1 => values[0] switch
                {
                    Null => new(new HashSet<ValueType>()),
                    Type type => type,
                    _ => throw new Throw($"'type' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'type' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString()
        {
            return "[type]";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value.Count);
        }

        public override bool Equals(object other)
        {
            if (other is Type type)
                return Value.SetEquals(type.Value);

            return false;
        }

        public Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call call)
        {
            if (Value.Count != 1)
                throw new Throw("Cannot instantiate a composite type");

            if (kwargs.Count != 0)
                throw new Throw("Constructors do not support named parameters");

            return Value.Single() switch
            {
                ValueType.Void => Void.Construct(args),
                ValueType.Null => Null.Construct(args),
                ValueType.Bool => Bool.Construct(args),
                ValueType.Number => Number.Construct(args),
                ValueType.Range => Range.Construct(args),
                ValueType.String => String.Construct(args),
                ValueType.Array => Array.Construct(args),
                ValueType.Struct => Struct.Construct(args),
                ValueType.Tuple => Tuple.Construct(args),
                ValueType.Func => Func.Construct(args),
                ValueType.Task => Task.Construct(args, call),
                ValueType.Iter => Iter.Construct(args, call),
                ValueType.Reference => Reference.Construct(args, call),
                ValueType.Extern => Extern.Construct(args),
                ValueType.Type => Construct(args),
                _ => throw new Exception()
            };
        }
    }
}