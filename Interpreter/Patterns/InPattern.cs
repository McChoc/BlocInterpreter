using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed record InPattern : IPatternNode
{
    private readonly Value _value;

    public InPattern(Value value)
    {
        _value = value;
    }

    public bool Matches(Value value, Call call)
    {
        var left = value;
        var right = ReferenceHelper.Resolve(_value, call.Engine.Options.HopLimit).Value;

        if (right is Array array)
            return array.Values.Any(v => v.Value.Equals(left));

        if (left is String sub && right is String str)
            return str.Value.Contains(sub.Value);

        return false;
    }

    public bool HasAssignment()
    {
        return false;
    }
}