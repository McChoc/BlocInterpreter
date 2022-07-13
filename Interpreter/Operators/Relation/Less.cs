using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Less : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Less(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call).Value;
            var rightValue = _right.Evaluate(call).Value;

            if (leftValue.Is(out Number? leftNumber) && rightValue.Is(out Number? rightNumber))
                return new Bool(leftNumber!.Value < rightNumber!.Value);

            throw new Throw($"Cannot apply operator '<' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");
        }
    }
}