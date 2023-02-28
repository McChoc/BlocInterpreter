using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
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

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            left = ReferenceUtil.Dereference(left, call.Engine.HopLimit).Value;
            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (left is Array array && right is Func func)
                return new Array(array.Variables
                    .Select(x => x.Value)
                    .Where(x => Bool.ImplicitCast(func.Invoke(new() { x.Copy() }, new(), call)).Value)
                    .ToList());

            throw new Throw($"Cannot apply operator 'where' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}