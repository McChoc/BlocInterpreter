using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record NotPattern : IPatternNode
{
    private readonly IPatternNode _pattern;

    public NotPattern(IPatternNode pattern)
    {
        _pattern = pattern;
    }

    public bool Matches(Value value, Call call)
    {
        return !_pattern.Matches(value, call);
    }

    public bool HasAssignment()
    {
        return _pattern.HasAssignment();
    }
}