using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class String : Value, IPattern, IIndexable
{
    public string Value { get; }

    public String(string value = "")
    {
        Value = value;
    }

    public IPatternNode GetRoot() => new RegexPattern(Value);
    public override ValueType GetType() => ValueType.String;
    public override string ToString() => $"\"{Value}\"";

    public IValue Index(Value value, Call _)
    {
        if (value is Number number)
        {
            int index = number.GetInt();

            if (index < 0)
                index += Value.Length;

            if (index < 0 || index >= Value.Length)
                throw new Throw("Index out of range");

            return new String(Value[index].ToString());
        }

        if (value is Range range)
        {
            var (start, end, step) = RangeHelper.Deconstruct(range, Value.Length);

            var builder = new StringBuilder();

            for (int i = start; i != end && end - i > 0 == step > 0; i += step)
                builder.Append(Value[i]);

            return new String(builder.ToString());
        }

        throw new Throw("It should be a number or a range.");
    }

    internal static String ImplicitCast(IValue value)
    {
        try
        {
            return Construct(new() { value.Value });
        }
        catch
        {
            throw new Throw($"Cannot implicitly convert '{value.Value.GetTypeName()}' to 'string'");
        }
    }

    internal static bool TryImplicitCast(IValue value, out String @string)
    {
        try
        {
            @string = Construct(new() { value.Value });
            return true;
        }
        catch
        {
            @string = null!;
            return false;
        }
    }

    internal static String Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [String @string] => @string,
            [Void] => throw new Throw($"'string' does not have a constructor that takes a 'void'"),
            [var value] => new(value.ToString()),
            [Number number, String format] => new(number.Value.ToString(format.Value, CultureInfo.InvariantCulture)), // TODO check formats
            [String separator, Array array] => new(string.Join(separator.Value, array.Values.Select(x => ImplicitCast(x.Value).Value))),
            [var value, Number count] => new(string.Concat(Enumerable.Repeat(ImplicitCast(value).Value, count.GetInt()))),
            [_, _] => throw new Throw($"'string' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [..] => throw new Throw($"'string' does not have a constructor that takes {values.Count} arguments")
        };
    }
}