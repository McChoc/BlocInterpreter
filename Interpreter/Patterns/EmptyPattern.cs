using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed record EmptyPattern : IPatternNode
{
    public bool Matches(Value value, Call call)
    {
        return value switch
        {
            Array array => array.Values.Count == 0,
            Struct @struct => @struct.Values.Count == 0,
            _ => false
        };
    }

    public bool HasAssignment()
    {
        return false;
    }
}