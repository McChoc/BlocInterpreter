using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class ExpressionStatement : Statement
{
    internal IExpression Expression { get; }

    internal ExpressionStatement(IExpression expression)
    {
        Expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var _, out var exception))
            yield return exception;
    }
}