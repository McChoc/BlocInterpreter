using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;

namespace CmmInterpretor.Statements
{
    public class ExpressionStatement : Statement
    {
        public IExpression Expression { get; }

        public ExpressionStatement(IExpression expression) => Expression = expression;

        public override Result? Execute(Call call)
        {
            try
            {
                var _ = Expression.Evaluate(call).Value;
            }
            catch (Result result)
            {
                return result;
            }

            return null;
        }

        public Variant<Value, Result> Evaluate(Call call)
        {
            try
            {
                return Expression.Evaluate(call).Value;
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}
