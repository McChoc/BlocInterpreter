using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BooleanXorAssignment : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanXorAssignment(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);

            if (leftValue is not Pointer pointer)
                throw new Throw("You cannot assign a value to a literal");

            if (!pointer.Get().Is(out Bool? leftBool))
                throw new Throw("Cannot implicitly convert to bool");

            var rightValue = _right.Evaluate(call);

            if (!rightValue.Value.Is(out Bool? rightBool))
                throw new Throw("Cannot implicitly convert to bool");

            Value value;

            if (leftBool!.Value == rightBool!.Value)
                value = Null.Value;
            else if (rightBool.Value)
                value = rightValue.Value;
            else
                return leftValue.Value;

            return pointer.Set(value);
        }
    }
}