using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
    public override string ToString() => @$"""{Value}""";

    public IValue Index(List<Value> args, Call _)
    {
        return args switch
        {
            [Number number] => MakeString(IndexByNumber(number)),
            [Range range] => MakeString(IndexByRange(range)),
            [Tuple tuple] => MakeString(IndexByTuple(tuple)),
            [_] => throw new Throw($"The string indexer does not takes a '{args[0].GetTypeName()}'"),
            [..] => throw new Throw($"The string indexer does not take {args.Count} arguments"),
        };
    }

    private String MakeString(char character)
    {
        return new String(character.ToString());
    }

    private String MakeString(IEnumerable<char> characters)
    {
        return new String(new string(characters.ToArray()));
    }

    private char IndexByNumber(Number number)
    {
        int index = number.GetInt();

        if (index < 0)
            index += Value.Length;

        if (index < 0 || index >= Value.Length)
            throw new Throw("Index out of range");

        return Value[index];
    }

    private IEnumerable<char> IndexByRange(Range range)
    {
        var (start, stop, step) = RangeHelper.Deconstruct(range, Value.Length);

        for (int i = start; i != stop && i < stop == step > 0; i += step)
            yield return Value[i];
    }

    private IEnumerable<char> IndexByTuple(Tuple tuple)
    {
        foreach (var index in tuple.Values)
        {
            if (index.Value is Number number)
            {
                yield return IndexByNumber(number);
                continue;
            }

            var values = index.Value switch
            {
                Range range => IndexByRange(range),
                Tuple nested => IndexByTuple(nested),
                _ => throw new Throw($"The string indexer does not takes a '{index.Value.GetTypeName()}'")
            };

            foreach (var value in values)
                yield return value;
        }
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

    internal static bool TryImplicitCast(IValue value, [NotNullWhen(true)] out String? @string)
    {
        try
        {
            @string = Construct(new() { value.Value });
            return true;
        }
        catch
        {
            @string = null;
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