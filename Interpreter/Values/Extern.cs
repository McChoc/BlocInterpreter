using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Extern : Value
    {
        public Extern() => Value = null;

        public Extern(object? value) => Value = value;

        public object? Value { get; }

        internal override ValueType GetType() => ValueType.Extern;

        public override bool Equals(Value other)
        {
            if (other is Extern complex)
                return Value == complex.Value;

            return false;
        }

        internal static Extern Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Null => new(),
                    Extern @extern => @extern,
                    _ => throw new Throw($"'extern' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'extern' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString(int _)
        {
            return "[extern]";
        }
    }
}