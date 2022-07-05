using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class ThrowStatement : Statement
    {
        private readonly IExpression? _expression;

        internal ThrowStatement() { }

        internal ThrowStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override Result Execute(Call call)
        {
            try
            {
                if (_expression is null)
                    return new Throw();

                var value = _expression.Evaluate(call);

                return new Throw(value.Value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}