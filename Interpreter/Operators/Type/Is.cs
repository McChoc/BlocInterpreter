using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Is : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Is(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call).Value;
            var rightValue = _right.Evaluate(call).Value;

            if (rightValue.Is(out Type? type))
                return new Bool(type!.Value.Contains(leftValue.GetType()));

            throw new Throw($"Cannot apply operator 'is' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");
        }
    }
}