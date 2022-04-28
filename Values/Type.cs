using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Type : Value
    {
        public static Type Any => new(Enum.GetValues(typeof(VariableType)).Cast<VariableType>().ToHashSet());

        public HashSet<VariableType> Value { get; set; }

        public Type() => Value = new HashSet<VariableType>();
        public Type(VariableType type) => Value = new HashSet<VariableType>() { type };
        public Type(HashSet<VariableType> types) => Value = types;

        public override VariableType TypeOf() => VariableType.Type;

        public override Value Copy() => new Type(new HashSet<VariableType>(Value));

        public override bool Equals(Value other)
        {
            if (other is Type t)
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

            if (typeof(T) == typeof(Type))
            {
                value = this as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.True;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            if (typeof(T) == typeof(Type))
                return this;

            return new Throw($"Cannot cast type as {typeof(T)}");
        }

        public override string ToString(int _)
        {
            if (Value.Count == 0)
                return "~any";
            else if (Value.Count == Enum.GetValues(typeof(VariableType)).Length)
                return "any";
            else
                return string.Join(" | ", Value).ToLower();
        }
    }
}
