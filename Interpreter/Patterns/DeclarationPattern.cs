using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record DeclarationPattern : IPatternNode
{
    private readonly IPatternNode? _pattern;
    private readonly IIdentifier _identifier;

    public DeclarationPattern(IPatternNode? pattern, IIdentifier identifier)
    {
        _pattern = pattern;
        _identifier = identifier;
    }

    public bool Matches(Value value, Call call)
    {
        if (_pattern is not null && !_pattern.Matches(value, call))
            return false;

        _identifier.Define(value, call, true, true);

        return true;
    }

    public bool HasAssignment()
    {
        return true;
    }
}