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

        public override bool Equals(Value other)
        {
            if (other is Type type)
                return Value.SetEquals(type.Value);

            return false;
        }

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

        public override string ToString(int _)
        {
            //if (Value.Count == 0)
            //    return "[empty type]";

            //return string.Join(" | ", Value).ToLower();

            return "[type]";
        }

        public Value Invoke(List<Value> values, Call call)
        {
            if (Value.Count != 1)
                throw new Throw("Cannot instantiate a composite type");

            return Value.Single() switch
            {
                ValueType.Void =>       Void.Construct(values),
                ValueType.Null =>       Null.Construct(values),
                ValueType.Bool =>       Bool.Construct(values),
                ValueType.Number =>     Number.Construct(values),
                ValueType.Range =>      Range.Construct(values),
                ValueType.String =>     String.Construct(values),
                ValueType.Array =>      Array.Construct(values),
                ValueType.Struct =>     Struct.Construct(values),
                ValueType.Tuple =>      Tuple.Construct(values),
                ValueType.Func =>       Func.Construct(values),
                ValueType.Task =>       Task.Construct(values, call),
                ValueType.Iter =>       Iter.Construct(values, call),
                ValueType.Reference =>  Reference.Construct(values, call),
                ValueType.Extern =>     Extern.Construct(values),
                ValueType.Type =>       Construct(values),
                _ => throw new Exception()
            };
        }
    }
}