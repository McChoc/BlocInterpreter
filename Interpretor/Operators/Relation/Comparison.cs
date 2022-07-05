using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using static System.Math;

namespace Bloc.Operators.Relation
{
    internal class Comparison : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Comparison(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (leftValue.Is(out Number? leftNumber) && rightValue.Is(out Number? rightNumber))
            {
                if (double.IsNaN(leftNumber!.Value) || double.IsNaN(rightNumber!.Value))
                    return new Number(double.NaN);

                return new Number(Sign(leftNumber!.Value - rightNumber!.Value));
            }

            throw new Throw($"Cannot apply operator '<=>' on operands of types {leftValue.Type.ToString().ToLower()} and {rightValue.Type.ToString().ToLower()}");
        }
    }
}