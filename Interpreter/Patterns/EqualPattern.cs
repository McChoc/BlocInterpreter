using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record EqualPattern : IPatternNode
{
    private readonly Value _value;

    public EqualPattern(Value value)
    {
        _value = value;
    }

    public bool Matches(Value value, Call call)
    {
        return value.Equals(_value);
    }

    public bool HasAssignment()
    {
        return false;
    }
}