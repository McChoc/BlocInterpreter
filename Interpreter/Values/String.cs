using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;

namespace Bloc.Values;

public sealed class String : Value, IIndexable
{
    public string Value { get; }

    public String() => Value = "";

    public String(string value) => Value = value;

    internal override ValueType GetType() => ValueType.String;

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
        return HashCode.Combine(Value);
    }

    public override bool Equals(object other)
    {
        return other is String @string &&
            Value == @string.Value;
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
            var (start, end) = RangeHelper.GetStartAndEnd(range, Value.Length);

            var builder = new StringBuilder();

            for (int i = start; i != end && end - i > 0 == range.Step > 0; i += range.Step)
                builder.Append(Value[i]);

            return new String(builder.ToString());
        }

        throw new Throw("It should be a number or a range.");
    }
}