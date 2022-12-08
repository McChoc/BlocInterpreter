using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Select : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Select(IExpression left, IExpression right)
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
                    .Select(x => func.Invoke(new() { x.Value.Copy() }, new(), call))
                    .ToList());

            throw new Throw($"Cannot apply operator 'select' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");
        }
    }
}