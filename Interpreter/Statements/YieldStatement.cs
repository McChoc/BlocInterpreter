using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class YieldStatement : Statement
{
    private readonly IExpression _expression;

    internal YieldStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
            yield return exception;
        else if (value is Void)
            yield return new Throw("'void' cannot be yielded");
        else
            yield return new Yield(value.GetOrCopy());
    }
}