using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class GotoCaseStatement : Statement
{
    private readonly IExpression _expression;

    internal GotoCaseStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
            yield return exception;
        else
            yield return new GotoCase(value.GetOrCopy());
    }
}