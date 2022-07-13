using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Substraction : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Substraction(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call);
            var right = _right.Evaluate(call);

            return TupleUtil.RecursivelyCall(left, right, Operation);
        }

        internal static IPointer Operation(IPointer left, IPointer right)
        {
            if (left.Value.Is(out Number? leftNumber) && right.Value.Is(out Number? rightNumber))
                return new Number(leftNumber!.Value - rightNumber!.Value);

            throw new Throw($"Cannot apply operator '-' on operands of types {left.Value.GetType().ToString().ToLower()} and {right.Value.GetType().ToString().ToLower()}");
        }
    }
}