using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record ThrowStatement : Statement
    {
        private readonly IExpression _expression;

        internal ThrowStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override Result Execute(Call call)
        {
            try
            {
                var value = _expression.Evaluate(call).Value;

                return new Throw(value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}