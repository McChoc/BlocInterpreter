using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class YieldManyStatement : Statement
{
    private readonly IExpression _expression;

    internal YieldManyStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
        {
            yield return exception;
            yield break;
        }
        
        if (!Iter.TryImplicitCast(value, out var iter, call))
        {
            yield return new Throw("Cannot implicitly convert to iter");
            yield break;
        }

        foreach (var item in iter.Iterate())
            yield return new Yield(item.GetOrCopy());
    }
}