using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record LetOperator : IExpression
{
    private readonly IExpression _operand;

    internal LetOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var identifier = _operand.Evaluate(call);

        return VariableHelper.Define(identifier, Null.Value, call);
    }
}