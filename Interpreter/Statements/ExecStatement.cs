using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Parsers;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Utils.Attributes;
using Bloc.Utils.Exceptions;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class ExecStatement : Statement
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
}