using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Operators.Relation
{
    internal class Inequality : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Inequality(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            return new Bool(!leftValue.Value.Equals(rightValue!));
        }
    }
}