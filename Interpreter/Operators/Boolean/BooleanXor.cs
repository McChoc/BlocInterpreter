using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
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

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;

            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;

            if (!left.Is(out Bool? leftBool))
                throw new Throw("Cannot implicitly convert to bool");

            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (!right.Is(out Bool? rightBool))
                throw new Throw("Cannot implicitly convert to bool");

            if (leftBool!.Value && !rightBool!.Value)
                return left;

            if (!leftBool!.Value && rightBool!.Value)
                return right;

            return Null.Value;
        }
    }
}