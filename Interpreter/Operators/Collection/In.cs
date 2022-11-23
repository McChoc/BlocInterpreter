using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record In : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal In(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (right is Array array)
                return new Bool(array.Values.Any(v => v.Value.Equals(left)));

            if (left is String sub && right is String str)
                return new Bool(str.Value.Contains(sub.Value));

            throw new Throw($"Cannot apply operator 'in' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}