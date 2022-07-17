using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class As : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal As(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            right = ReferenceUtil.Dereference(right, call.Engine.HopLimit).Value;

            if (!right.Is(out Type? type))
                throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetType().ToString().ToLower()} and {right.GetType().ToString().ToLower()}");

            if (type!.Value.Count != 1)
                throw new Throw("Cannot apply operator 'as' on a composite type");

            return left.Explicit(type.Value.Single());
        }
    }
}