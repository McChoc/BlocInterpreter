using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Expressions.Operators;

internal sealed record AdditionAssignment : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal AdditionAssignment(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call);
        var right = _right.Evaluate(call);

        return AssignmentHelper.CompoundAssign(left, right, AdditionOperator.Operation, call);
    }
}