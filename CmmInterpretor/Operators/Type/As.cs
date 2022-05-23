using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Linq;

namespace CmmInterpretor.Operators.Type
{
    public class As : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public As(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var leftValue = _left.Evaluate(call);
            var rightValue = _right.Evaluate(call);

            if (!rightValue.Is(out TypeCollection? type))
                throw new Throw($"Cannot apply operator 'as' on operands of types {leftValue.Type.ToString().ToLower()} and {rightValue.Type.ToString().ToLower()}");

            if (type!.Value.Count != 1)
                throw new Throw($"Cannot apply operator 'as' on a composite type");

            return type.Value.Single() switch
            {
                ValueType.Void => Void.Value,
                ValueType.Null => Null.Value,
                ValueType t => leftValue.Explicit(t)
            };
        }
    }
}
