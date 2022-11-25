using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Where : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Where(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;
            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (left is Array array && right is Func func)
                return new Array(array.Values
                    .Where(x => Bool.ImplicitCast(func.Invoke(new() { x.Value.Copy() }, call)).Value)
                    .ToList());

            throw new Throw($"Cannot apply operator 'where' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}