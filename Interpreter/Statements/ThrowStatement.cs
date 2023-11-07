using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class ThrowStatement : Statement
{
    private readonly IExpression _expression;

    internal ThrowStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
            yield return exception;
        else if (value is Void)
            yield return new Throw("'void' cannot be thrown");
        else
            yield return new Throw(value.GetOrCopy());
    }
}