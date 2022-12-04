using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class YieldStatement : Statement
    {
        private readonly IExpression _expression;

        internal YieldStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (value, exception) = EvaluateExpression(_expression, call);

            if (exception is not null)
                yield return exception;
            else
                yield return new Yield(value!.Value);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, _expression);
        }

        public override bool Equals(object other)
        {
            return other is YieldStatement statement &&
                Label == statement.Label &&
                _expression.Equals(statement._expression);
        }
    }
}