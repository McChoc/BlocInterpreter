using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;

namespace Bloc.Values
{
    public sealed class String : Value, IIndexable
    {
        public String() => Value = "";

        public String(string value) => Value = value;

        public string Value { get; }

        internal override ValueType GetType() => ValueType.String;

        internal static String Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(),
                1 => values[0] switch
                {
                    Void => throw new Throw($"'string' does not have a constructor that takes a 'void'"),
                    Null => new(),
                    String @string => @string,
                    _ => new(values[0].ToString())
                },
                2 => (values[0], values[1]) switch
                {
                    (Number number, String format) => new(number.Value.ToString(format.Value, CultureInfo.InvariantCulture)), // TODO check formats
                    (String separator, Array array) => new(string.Join(separator.Value, array.Variables.Select(x => ImplicitCast(x.Value).Value))),
                    (var value, Number count) => new(string.Concat(Enumerable.Repeat(ImplicitCast(value).Value, count.GetInt()))),
                    _ => throw new Throw($"'string' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}' and a '{values[1].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'string' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal static String ImplicitCast(Value value)
        {
            try
            {
                return Construct(new() { value });
            }
            catch
            {
                throw new Throw($"Cannot implicitly convert '{value.GetType().ToString().ToLower()}' to 'string'");
            }
        }

        internal static bool TryImplicitCast(Value value, out String @string)
        {
            try
            {
                @string = Construct(new() { value });
                return true;
            }
            catch
            {
                @string = null!;
                return false;
            }
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Value);
        }

        public override bool Equals(object other)
        {
            if (other is String @string)
                return Value == @string.Value;

            return false;
        }

        public IValue Index(Value value, Call _)
        {
            if (value is Number number)
            {
                var index = number.GetInt();

                if (index < 0)
                    index += Value.Length;

                if (index < 0 || index >= Value.Length)
                    throw new Throw("Index out of range");

                return new String(Value[index].ToString());
            }

            if (value is Range range)
            {
                var (start, end) = RangeUtil.GetStartAndEnd(range, Value.Length);

                var builder = new StringBuilder();

                for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                    builder.Append(Value[i]);

                return new String(builder.ToString());
            }

            throw new Throw("It should be a number or a range.");
        }
    }
}