using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record TypePattern : IPatternNode
{
    private readonly ValueType _type;

    public TypePattern(ValueType type)
    {
        _type = type;
    }

    public bool Matches(Value value, Call _)
    {
        return value.GetType() == _type;
    }

    public bool HasAssignment()
    {
        return false;
    }
}