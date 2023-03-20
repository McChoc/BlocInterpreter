using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record BitwiseOrAssignment : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal BitwiseOrAssignment(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call);
        var right = _right.Evaluate(call).Value;

        return AssignmentHelper.CompoundAssign(left, right, BitwiseOrOperator.Operation, call);
    }
}