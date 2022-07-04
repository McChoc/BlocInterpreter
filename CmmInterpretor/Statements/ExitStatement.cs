using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
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

                var value = _expression.Evaluate(call);

                return new Exit(value.Value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}