using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal sealed record YieldStatement : Statement
    {
        private readonly IExpression _expression;

        internal YieldStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override Result Execute(Call call)
        {
            try
            {
                var value = _expression.Evaluate(call).Value;

                return new Yield(value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}