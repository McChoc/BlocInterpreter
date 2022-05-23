using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Relation
{
    public class Equality : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public Equality(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            return new Bool(leftValue!.Equals(rightValue!));
        }
    }
}
