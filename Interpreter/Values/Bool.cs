using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Bool : Value, IScalar
    {
        internal Bool(bool value) => Value = value;

        public static Bool True { get; } = new(true);
        public static Bool False { get; } = new(false);

        public bool Value { get; }

        public int GetInt() => Value ? 1 : 0;

        public double GetDouble() => Value ? 1 : 0;

        internal override ValueType GetType() => ValueType.Bool;

        public override bool Equals(Value other)
        {
            if (other is Bool @bool)
                return Value == @bool.Value;

            return false;
        }

        internal static Bool Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => False,
                1 => values[0] switch
                {
                    Void => throw new Throw($"'bool' does not have a constructor that takes a 'void'"),
                    Null => False,
                    Bool @bool => @bool,
                    Number number => new(number.Value is not (0 or double.NaN)),
                    _ => True
                },
                _ => throw new Throw($"'bool' does not have a constructor that takes {values.Count} arguments"),
            };
        }

        internal static Bool ImplicitCast(Value value)
        {
            try
            {
                return Construct(new() { value });
            }
            catch
            {
                throw new Throw($"Cannot implicitly convert '{value.GetType().ToString().ToLower()}' to 'bool'");
            }
        }

        internal static bool TryImplicitCast(Value value, out Bool @bool)
        {
            try
            {
                @bool = Construct(new() { value });
                return true;
            }
            catch
            {
                @bool = null!;
                return false;
            }
        }

        public override string ToString(int _) => Value ? "true" : "false";
    }
}