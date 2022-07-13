using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
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
            var leftValue = _left.Evaluate(call).Value;
            var rightValue = _right.Evaluate(call).Value;

            if (!rightValue.Is(out Type? type))
                throw new Throw($"Cannot apply operator 'as' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");

            if (type!.Value.Count != 1)
                throw new Throw("Cannot apply operator 'as' on a composite type");

            return leftValue.Explicit(type.Value.Single());
        }
    }
}