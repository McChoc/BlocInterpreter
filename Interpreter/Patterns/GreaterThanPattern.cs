using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed record GreaterThanPattern : IPatternNode
{
    private readonly Value _value;

    public GreaterThanPattern(Value value)
    {
        _value = value;
    }

    public bool Matches(Value value, Call call)
    {
        var left = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
        var right = ReferenceHelper.Resolve(_value, call.Engine.Options.HopLimit).Value;

        if (left is INumeric leftNumeric && right is INumeric rightNumeric)
            return leftNumeric.GetDouble() > rightNumeric.GetDouble();

        if (left is String leftString && right is String rightString)
            return string.CompareOrdinal(leftString.Value, rightString.Value) > 0;

        return false;
    }

    public bool HasAssignment()
    {
        return false;
    }
}