using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Relation
{
    internal class Equality : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Equality(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            return new Bool(leftValue.Equals(rightValue));
        }
    }
}