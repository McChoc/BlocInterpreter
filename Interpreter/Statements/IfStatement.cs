using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class IfStatement : Statement
    {
        internal IExpression Expression { get; set; } = null!;
        internal List<Statement> Then { get; set; } = new();
        internal List<Statement> Else { get; set; } = new();

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (value, exception) = EvaluateExpression(Expression, call);

            if (exception is not null)
            {
                yield return exception;
                yield break;
            }

            if (!Bool.TryImplicitCast(value!.Value, out var @bool))
            {
                yield return new Throw("Cannot implicitly convert to bool");
                yield break;
            }

            var statements = @bool.Value ? Then : Else;
            var labels = StatementUtil.GetLabels(statements);

            try
            {
                call.Push();

                foreach (var result in ExecuteBlock(statements, labels, call))
                {
                    yield return result;

                    if (result is not Yield)
                        yield break;
                }
            }
            finally
            {
                call.Pop();
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Expression, Then.Count, Else.Count);
        }

        public override bool Equals(object other)
        {
            return other is IfStatement statement &&
                Label == statement.Label &&
                Expression.Equals(statement.Expression) &&
                Then.SequenceEqual(statement.Then) &&
                Else.SequenceEqual(statement.Else);
        }
    }
}