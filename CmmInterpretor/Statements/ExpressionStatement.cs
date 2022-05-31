using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;

namespace CmmInterpretor.Statements
{
    internal class ExpressionStatement : Statement
    {
        internal IExpression Expression { get; }

        internal ExpressionStatement(IExpression expression) => Expression = expression;

        internal override Result? Execute(Call call)
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

        internal Variant<Value, Result> Evaluate(Call call)
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
