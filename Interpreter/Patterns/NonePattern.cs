using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record NonePattern : IPatternNode
{
    public bool Matches(Value _0, Call _1)
    {
        return false;
    }

    public bool HasAssignment()
    {
        return false;
    }
}