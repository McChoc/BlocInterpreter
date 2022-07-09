﻿using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using static System.Math;

namespace Bloc.Operators.Arithmetic
{
    internal class Root : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Root(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(left, right, Operation);
        }

        internal static IValue Operation(IValue left, IValue right)
        {
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(Pow(leftNumber!.Value, 1 / rightNumber!.Value));

            throw new Throw($"Cannot apply operator '//' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}