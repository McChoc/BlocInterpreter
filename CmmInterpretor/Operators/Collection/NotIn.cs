using System.Linq;
using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Collection
{
    internal class NotIn : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal NotIn(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (rightValue.Is(out Array? array))
                return new Bool(!array!.Values.Any(v => v.Equals(leftValue)));

            if (leftValue.Is(out String? sub) && rightValue.Is(out String? str))
                return new Bool(!str!.Value.Contains(sub!.Value));

            throw new Throw($"Cannot apply operator 'not in' on operands of types {leftValue.Type.ToString().ToLower()} and {rightValue.Type.ToString().ToLower()}");
        }
    }
}