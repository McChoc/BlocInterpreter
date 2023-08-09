using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Patterns;

internal sealed record PredicatePattern : IPatternNode
{
    private readonly Func _predicate;

    public PredicatePattern(Func predicate)
    {
        _predicate = predicate;
    }

    public bool Matches(Value value, Call call)
    {
        var result = _predicate.Invoke(new() { value }, new(), call);

        return Bool.ImplicitCast(result).Value;
    }

    public bool HasAssignment()
    {
        return false;
    }
}