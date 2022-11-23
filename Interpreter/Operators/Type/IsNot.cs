using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record IsNot : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal IsNot(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (right is Type type)
                return new Bool(!type.Value.Contains(left.GetType()));

            throw new Throw($"Cannot apply operator 'is not' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}