using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

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
        Assign(result, value);
        return true;
    }

    public bool HasAssignment()
    {
        return true;
    }

    private static Value Assign(IValue left, IValue right)
    {
        var value = right.Value.GetOrCopy();

        switch (left)
        {
            case Pointer pointer:
                return pointer.Set(value);

            case Tuple { Assignable: true } tuple:
                if (value is not Tuple rightTuple)
                    return new Tuple(tuple.Values.Select(x => Assign(x, value)).ToList());

                if (tuple.Values.Count == rightTuple.Values.Count)
                    return new Tuple(tuple.Values.Zip(rightTuple.Values, Assign).ToList());

                throw new Throw("Miss mathch number of elements inside the tuples");

            default:
                throw new Throw("The right part of an assignment pattern must be assignable");
        };
    }
}