﻿using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Greater : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Greater(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;
            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Bool(leftNumber!.Value > rightNumber!.Value);

            throw new Throw($"Cannot apply operator '>' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}