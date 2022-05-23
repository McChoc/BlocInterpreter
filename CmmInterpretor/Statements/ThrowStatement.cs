using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class ThrowStatement : Statement
    {
        private readonly IExpression? _expression;

        public ThrowStatement() { }
        public ThrowStatement(IExpression expression) => _expression = expression;

        public override Result Execute(Call call)
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
