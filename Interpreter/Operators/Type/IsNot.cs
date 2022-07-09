using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Type
{
    internal class IsNot : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal IsNot(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (rightValue.Value.Is(out Values.Type? type))
                return new Bool(!type!.Value.Contains(leftValue.Value.GetType()));

            throw new Throw($"Cannot apply operator 'is not' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");
        }
    }
}