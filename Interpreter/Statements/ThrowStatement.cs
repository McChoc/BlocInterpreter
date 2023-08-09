using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Void = Bloc.Values.Types.Void;

namespace Bloc.Statements;

internal sealed class ThrowStatement : Statement
{
    private readonly IExpression _expression;

    internal ThrowStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
            yield return exception!;
        else if (value is Void)
            yield return new Throw("'void' cannot be thrown");
        else
            yield return new Throw(value!.GetOrCopy());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _expression);
    }

    public override bool Equals(object other)
    {
        return other is ThrowStatement statement &&
            Label == statement.Label &&
            _expression.Equals(statement._expression);
    }
}