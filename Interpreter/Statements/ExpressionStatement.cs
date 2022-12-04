using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class ExpressionStatement : Statement
    {
        internal ExpressionStatement(IExpression expression)
        {
            Expression = expression;
        }

        internal IExpression Expression { get; }

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (_, exception) = EvaluateExpression(Expression, call);

            if (exception is not null)
                yield return exception;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Expression);
        }

        public override bool Equals(object other)
        {
            return other is ExpressionStatement statement &&
                Label == statement.Label &&
                Expression.Equals(statement.Expression);
        }
    }
}