using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BitwiseXor : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BitwiseXor(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(leftValue, rightValue, Operation);
        }

        internal static IPointer Operation(IPointer left, IPointer right)
        {
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(leftNumber!.ToInt() ^ rightNumber!.ToInt());

            if (left.Value.Is(out Type? leftType) && right.Value.Is(out Type? rightType))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType type in System.Enum.GetValues(typeof(ValueType)))
                    if (leftType!.Value.Contains(type) != rightType!.Value.Contains(type))
                        types.Add(type);

                return new Type(types);
            }

            throw new Throw($"Cannot apply operator '^' on operands of types {left.Value.GetType().ToString().ToLower()} and {right.Value.GetType().ToString().ToLower()}");
        }
    }
}