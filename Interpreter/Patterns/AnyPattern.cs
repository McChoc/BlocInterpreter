using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record AnyPattern : IPatternNode
{
    public bool Matches(Value _0, Call _1)
    {
        return true;
    }

    public bool HasAssignment()
    {
        return false;
    }
}