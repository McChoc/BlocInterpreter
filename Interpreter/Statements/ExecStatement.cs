using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class ExecStatement : Statement
    {
        private readonly IExpression _expression;

        internal ExecStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (value, exception) = EvaluateExpression(_expression, call);

            if (exception is not null)
            {
                yield return exception;
                yield break;
            }
            
            if (value!.Value is not String @string)
            {
                yield return new Throw("The expression of an 'exec' statement must be a 'string'");
                yield break;
            }

            List<Statement>? statements = null;
            Result? error = null;

            try
            {
                statements = StatementScanner.GetStatements(@string.Value);
            }
            catch (SyntaxError e)
            {
                error = new Throw(e.Text);
            }
            catch
            {
                error = new Throw("Failed to execute statements");
            }

            if (error is not null)
            {
                yield return error;
                yield break;
            }

            var labels = StatementUtil.GetLabels(statements!);

            foreach (var result in ExecuteBlock(statements!, labels, call))
            {
                yield return result;

                if (result is not Yield)
                    yield break;
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, _expression);
        }

        public override bool Equals(object other)
        {
            return other is ExecStatement statement &&
                Label == statement.Label &&
                _expression.Equals(statement._expression);
        }
    }
}