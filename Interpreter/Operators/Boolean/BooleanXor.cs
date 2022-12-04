using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record BooleanXor : IExpression
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
            var left = _left.Evaluate(call).Value;

            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;

            var leftBool = Bool.ImplicitCast(left);

            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            var rightBool = Bool.ImplicitCast(right);

            if (leftBool.Value && !rightBool.Value)
                return left;

            if (!leftBool.Value && rightBool.Value)
                return right;

            return Null.Value;
        }
    }
}