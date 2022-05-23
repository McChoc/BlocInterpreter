using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Type
{
    public class IsNot : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public IsNot(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (rightValue.Is(out TypeCollection? type))
                return new Bool(!type!.Value.Contains(leftValue.Type));

            throw new Throw($"Cannot apply operator 'is not' on operands of types {leftValue.Type.ToString().ToLower()} and {rightValue.Type.ToString().ToLower()}");
        }
    }
}
