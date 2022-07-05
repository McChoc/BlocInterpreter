using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal class ExpressionStatement : Statement
    {
        internal ExpressionStatement(IExpression expression)
        {
            Expression = expression;
        }

        internal IExpression Expression { get; }

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