using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
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

                var value = _expression.Evaluate(call);

                return new Return(value.Value);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}