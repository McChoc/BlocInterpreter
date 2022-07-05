using System;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Boolean
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

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);

            if (!leftValue.Value.Is(out Bool? leftBool))
                throw new Throw("Cannot implicitly convert to bool");

            var rightValue = _right.Evaluate(call);

            if (!rightValue.Value.Is(out Bool? rightBool))
                throw new Throw("Cannot implicitly convert to bool");

            if (leftBool!.Value == rightBool!.Value)
                return Null.Value;

            if (leftBool.Value)
                return leftValue;

            if (rightBool.Value)
                return rightValue;

            throw new Exception();
        }
    }
}