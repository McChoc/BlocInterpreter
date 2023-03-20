using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record CatchOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal CatchOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        try
        {
            return _left.Evaluate(call).Value;
        }
        catch (Throw)
        {
            return _right.Evaluate(call).Value;
        }
    }
}