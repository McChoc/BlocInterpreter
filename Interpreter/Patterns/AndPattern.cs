using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record AndPattern : IPatternNode
{
    private readonly IPatternNode _left;
    private readonly IPatternNode _right;

    public AndPattern(IPatternNode left, IPatternNode right)
    {
        _left = left;
        _right = right;
    }

    public bool Matches(Value value, Call call)
    {
        return _left.Matches(value, call) && _right.Matches(value, call);
    }

    public bool HasAssignment()
    {
        return _left.HasAssignment() || _right.HasAssignment();
    }
}