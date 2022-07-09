using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Type
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

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (!rightValue.Value.Is(out Values.Type? type))
                throw new Throw(
                    $"Cannot apply operator 'as' on operands of types {leftValue.GetType().ToString().ToLower()} and {rightValue.GetType().ToString().ToLower()}");

            if (type!.Value.Count != 1)
                throw new Throw("Cannot apply operator 'as' on a composite type");

            return type.Value.Single() switch
            {
                ValueType.Void => Void.Value,
                ValueType.Null => Null.Value,
                ValueType t => leftValue.Value.Explicit(t)
            };
        }
    }
}