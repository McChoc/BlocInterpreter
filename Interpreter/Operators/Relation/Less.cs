using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Relation
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

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (leftValue.Value.Is(out Number? leftNumber) && rightValue.Value.Is(out Number? rightNumber))
                return new Bool(leftNumber!.Value < rightNumber!.Value);

            throw new Throw($"Cannot apply operator '<' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");
        }
    }
}