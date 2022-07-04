﻿using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Bitwise
{
    internal class LeftShift : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal LeftShift(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(leftValue, rightValue, Operation);
        }

        internal static IValue Operation(IValue left, IValue right)
        {
            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Number(leftNumber!.ToInt() << rightNumber!.ToInt());

            throw new Throw($"Cannot apply operator '<<' on operands of types {left.Type.ToString().ToLower()} and {right.Type.ToString().ToLower()}");
        }
    }
}