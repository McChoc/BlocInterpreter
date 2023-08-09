using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record MatchAssignmentOperator : IExpression
{
    private readonly IExpression? _left;
    private readonly IExpression _right;

    internal MatchAssignmentOperator(IExpression? left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        IPatternNode? root = null;

        if (_left is not null)
        {
            var value = _left.Evaluate(call).Value;

            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

            if (value is not IPattern pattern)
                throw new Throw($"Cannot apply operator '->' on operand of type {value.GetTypeName()}");

            root = pattern.GetRoot();
        }

        return new Pattern(new AssignmentPattern(root, _right));
    }
}