using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BooleanXor : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanXor(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call).Value;

            if (!leftValue.Is(out Bool? leftBool))
                throw new Throw("Cannot implicitly convert to bool");

            var rightValue = _right.Evaluate(call).Value;

            if (!rightValue.Is(out Bool? rightBool))
                throw new Throw("Cannot implicitly convert to bool");

            if (leftBool!.Value && !rightBool!.Value)
                return leftValue;

            if (!leftBool!.Value && rightBool!.Value)
                return rightValue;

            return Null.Value;
        }
    }
}