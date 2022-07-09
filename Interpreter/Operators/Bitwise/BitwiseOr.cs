using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Operators.Bitwise
{
    internal class BitwiseOr : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BitwiseOr(IExpression left, IExpression right)
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
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(leftNumber!.ToInt() | rightNumber!.ToInt());

            if (left.Value.Is(out Values.Type? leftType) && right.Value.Is(out Values.Type? rightType))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType type in Enum.GetValues(typeof(ValueType)))
                    if (leftType!.Value.Contains(type) || rightType!.Value.Contains(type))
                        types.Add(type);

                return new Values.Type(types);
            }

            throw new Throw($"Cannot apply operator '|' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}