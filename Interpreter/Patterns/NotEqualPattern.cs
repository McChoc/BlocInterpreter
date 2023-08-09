using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record NotEqualPattern : IPatternNode
{
    private readonly Value _value;

    public NotEqualPattern(Value value)
    {
        _value = value;
    }

    public bool Matches(Value value, Call call)
    {
        return !value.Equals(_value);
    }

    public bool HasAssignment()
    {
        return false;
    }
}