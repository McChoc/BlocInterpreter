using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Collection
{
    internal class In : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal In(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (rightValue.Value.Is(out Array? array))
                return new Bool(array!.Values.Any(v => v.Value.Equals(leftValue!)));

            if (leftValue.Value.Is(out String? sub) && rightValue.Value.Is(out String? str))
                return new Bool(str!.Value.Contains(sub!.Value));

            throw new Throw($"Cannot apply operator 'in' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");
        }
    }
}