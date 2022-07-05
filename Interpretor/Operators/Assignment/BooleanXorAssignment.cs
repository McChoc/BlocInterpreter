using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators.Assignment
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

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);

            if (leftValue is not Variable variable)
                throw new Throw("You cannot assign a value to a literal");

            if (!leftValue.Value.Is(out Bool? leftBool))
                throw new Throw("Cannot implicitly convert to bool");

            var rightValue = _right.Evaluate(call);

            if (!rightValue.Value.Is(out Bool? rightBool))
                throw new Throw("Cannot implicitly convert to bool");

            Value value = default!;

            if (leftBool!.Value == rightBool!.Value)
                value = Null.Value;
            else if (rightBool.Value)
                value = rightValue.Value;
            else if (leftBool.Value)
                return leftValue.Value;

            value!.Assign();
            variable.Value.Destroy();
            return variable.Value = value;
        }
    }
}