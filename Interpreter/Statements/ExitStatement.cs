using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements
{
    internal class ExitStatement : Statement
    {
        private readonly IExpression? _expression;

        internal ExitStatement() { }

        internal ExitStatement(IExpression expression)
        {
            _expression = expression;
        }

        internal override Result Execute(Call call)
        {
            try
            {
                if (_expression is null)
                    return new Exit();

                var value = _expression.Evaluate(call).Value;

                return new Exit(value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}