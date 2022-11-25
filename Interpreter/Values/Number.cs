using System.Collections.Generic;
using System.Globalization;
using Bloc.Interfaces;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Number : Value, IScalar
    {
        public Number(double value) => Value = value;

        public double Value { get; }

        internal override ValueType GetType() => ValueType.Number;

        public override bool Equals(Value other)
        {
            if (other is Number number)
                return Value == number.Value;

            return false;
        }

        internal static Number Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(0),
                1 => values[0] switch
                {
                    Null => new(0),
                    Bool @bool => new(@bool.Value ? 1 : 0),
                    Number @number => number,
                    String @string => @string.Value.Trim() switch
                    {
                        "nan" => new(double.NaN),
                        "infinity" => new(double.PositiveInfinity),
                        "-infinity" => new(double.NegativeInfinity),
                        var value => double.TryParse(value, out var result)
                            ? new(result)
                            : throw new Throw("Input string was not in a correct format") // TODO parse integers with 0x, 0o and 0b prefixes
                    },
                    var value => throw new Throw($"'number' does not have a constructor that takes a '{value.GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'number' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal static Number ImplicitCast(Value value)
        {
            try
            {
                return Construct(new() { value });
            }
            catch
            {
                throw new Throw($"Cannot implicitly convert '{value.GetType().ToString().ToLower()}' to 'number'");
            }
        }

        internal static bool TryImplicitCast(Value value, out Number number)
        {
            try
            {
                number = Construct(new() { value });
                return true;
            }
            catch
            {
                number = null!;
                return false;
            }
        }

        public int GetInt()
        {
            if (double.IsNaN(Value))
                return 0;

            if (Value > int.MaxValue)
                return int.MaxValue;

            if (Value < int.MinValue)
                return int.MinValue;

            return (int)Value;
        }

        public double GetDouble()
        {
            return Value;
        }

        public override string ToString(int _)
        {
            if (double.IsNaN(Value))
                return "nan";

            if (double.IsPositiveInfinity(Value))
                return "infinity";

            if (double.IsNegativeInfinity(Value))
                return "-infinity";

            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}