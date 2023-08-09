using System;
using System.Collections.Generic;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using ValueType = Bloc.Values.Core.ValueType;

namespace Bloc.Values.Types;

public sealed class Pattern : Value, IPattern
{
    public IPatternNode Value { get; }

    public Pattern(IPatternNode pattern) => Value = pattern;

    public IPatternNode GetRoot() => Value;

    internal override ValueType GetType() => ValueType.Pattern;

    internal static Pattern Construct(List<Value> values)
    {
        return values switch
        {
            [Pattern pattern] => pattern,
            [IPattern pattern] => new(pattern.GetRoot()),
            [_] => throw new Throw($"'pattern' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'pattern' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override string ToString()
    {
        return "[pattern]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override bool Equals(object other)
    {
        return other is Pattern pattern &&
            Value.Equals(pattern.Value);
    }
}