using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record BooleanOr : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanOr(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _left.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            var @bool = Bool.ImplicitCast(value);

            if (@bool.Value)
                return value;

            return _right.Evaluate(call).Value;
        }
    }
}