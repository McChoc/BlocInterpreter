using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed class ReturnStatement : Statement
    {
        private readonly IExpression? _expression;

        internal ReturnStatement() { }

        internal ReturnStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override IEnumerable<Result> Execute(Call call)
        {
            if (_expression is null)
            {
                yield return new Return();
            }
            else
            {
                var (value, exception) = EvaluateExpression(_expression, call);

                if (exception is not null)
                    yield return exception;
                else
                    yield return new Return(value!.Value);
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, _expression);
        }

        public override bool Equals(object other)
        {
            return other is ReturnStatement statement &&
                Label == statement.Label &&
                Equals(_expression, statement._expression);
        }
    }
}