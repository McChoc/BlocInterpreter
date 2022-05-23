using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;

namespace CmmInterpretor.Statements
{
    public class ReturnStatement : Statement
    {
        private readonly IExpression? _expression;

        public ReturnStatement() { }
        public ReturnStatement(IExpression expression) => _expression = expression;

        public override Result Execute(Call call)
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
