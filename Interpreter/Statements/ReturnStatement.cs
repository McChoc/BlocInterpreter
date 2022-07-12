using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class ReturnStatement : Statement
    {
        private readonly IExpression? _expression;

        internal ReturnStatement() { }

        internal ReturnStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override Result Execute(Call call)
        {
            try
            {
                if (_expression is null)
                    return new Return();

                var value = _expression.Evaluate(call).Value;

                return new Return(value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}