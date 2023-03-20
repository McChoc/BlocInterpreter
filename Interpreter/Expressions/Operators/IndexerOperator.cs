using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record IndexerOperator : IExpression
{
    private readonly IExpression _expression;
    private readonly IExpression _index;

    internal IndexerOperator(IExpression expression, IExpression index)
    {
        _expression = expression;
        _index = index;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is not IIndexable indexable)
            throw new Throw("The '[]' operator can only be apllied to a string, an array or a struct");

        var index = _index.Evaluate(call).Value;

        return indexable.Index(index, call);
    }
}