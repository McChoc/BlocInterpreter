using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Values
{
    public sealed class String : Value, IIndexable
    {
        public String() => Value = "";

        public String(string value) => Value = value;

        public string Value { get; }

        internal override ValueType GetType() => ValueType.String;

        public override bool Equals(Value other)
        {
            if (other is String @string)
                return Value == @string.Value;

            return false;
        }

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
                    (Number number, String format) => new(number.Value.ToString(format.Value)), // TODO check formats
                    (String @string, Array array) => new(string.Join(@string.Value, array.Values.Select(x => ImplicitCast(x.Value).Value))),
                    (var value, Number number) => new(string.Concat(Enumerable.Repeat(value.ToString(), number.GetInt()))),
                    var value => throw new Throw($"'string' does not have a constructor that takes a '{value.Item1.GetType().ToString().ToLower()}' and a '{value.Item2.GetType().ToString().ToLower()}'")
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

        public IPointer Index(Value value, Call _)
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

        public override string ToString(int _)
        {
            return $"\"{Value}\"";
        }
    }
}