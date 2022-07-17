using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
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

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value left, Value right)
        {
            if (left.Is(out Number? leftNumber) && right.Is(out Number? rightNumber))
                return new Number(leftNumber!.ToInt() | rightNumber!.ToInt());

            if (left.Is(out Type? leftType) && right.Is(out Type? rightType))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType type in System.Enum.GetValues(typeof(ValueType)))
                    if (leftType!.Value.Contains(type) || rightType!.Value.Contains(type))
                        types.Add(type);

                return new Type(types);
            }

            throw new Throw($"Cannot apply operator '|' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}