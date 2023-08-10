using System.Collections.Generic;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Pattern : Value, IPattern
{
    public IPatternNode Value { get; }

    public Pattern(IPatternNode pattern)
    {
        Value = pattern;
    }

    public IPatternNode GetRoot() => Value;
    public override ValueType GetType() => ValueType.Pattern;
    public override string ToString() => "[pattern]";

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
}