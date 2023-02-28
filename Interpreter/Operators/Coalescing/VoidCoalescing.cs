using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record VoidCoalescing : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal VoidCoalescing(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var value = _left.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is Void)
                return _right.Evaluate(call).Value;

            return value;
        }
    }
}