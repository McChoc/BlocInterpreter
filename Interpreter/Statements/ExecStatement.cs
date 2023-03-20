using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Parsers;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Utils.Exceptions;
using String = Bloc.Values.String;

namespace Bloc.Statements;

internal sealed class ExecStatement : Statement
{
    private readonly IExpression _expression;

    internal ExecStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(_expression, call, out var value, out var exception))
        {
            yield return exception!;
            yield break;
        }

        if (!String.TryImplicitCast(value!, out var @string))
        {
            yield return new Throw("Cannot implicitly convert to string");
            yield break;
        }

        List<Statement>? statements = null;

        try
        {
            var tokenizer = new Tokenizer(@string.Value);
            statements = StatementParser.Parse(tokenizer);
        }
        catch (SyntaxError e)
        {
            exception = new Throw(e.Text);
        }
        catch
        {
            exception = new Throw("Failed to execute statements");
        }

        if (exception is not null)
        {
            yield return exception;
            yield break;
        }

        foreach (var result in ExecuteStatements(statements!, call))
        {
            yield return result;

            if (result is not Yield)
                yield break;
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _expression);
    }

    public override bool Equals(object other)
    {
        return other is ExecStatement statement &&
            Label == statement.Label &&
            _expression.Equals(statement._expression);
    }
}