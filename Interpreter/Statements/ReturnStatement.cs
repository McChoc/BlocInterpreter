using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class ReturnStatement : Statement
{
    private readonly IExpression? _expression;

    internal ReturnStatement() { }

    internal ReturnStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (_expression is null)
            yield return new Return();
        else if (EvaluateExpression(_expression, call, out var value, out var exception))
            yield return new Return(value!.GetOrCopy());
        else
            yield return exception!;
    }
}