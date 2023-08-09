using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;

namespace Bloc.Patterns;

internal sealed record AssignmentPattern : IPatternNode
{
    private readonly IPatternNode? _pattern;
    private readonly IExpression _expression;

    public AssignmentPattern(IPatternNode? pattern, IExpression expression)
    {
        _pattern = pattern;
        _expression = expression;
    }

    public bool Matches(Value value, Call call)
    {
        if (_pattern is not null && !_pattern.Matches(value, call))
            return false;

        var result = _expression.Evaluate(call);

        if (result is not Pointer pointer)
            throw new Throw("The value must be assignable");

        pointer.Set(value);

        return true;
    }

    public bool HasAssignment()
    {
        return true;
    }
}