using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Boolean
{
    internal class BooleanAnd : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanAnd(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var value = _left.Evaluate(call);

            if (!value.Is(out Bool? @bool))
                throw new Throw("Cannot implicitly convert to bool");

            if (!@bool!.Value)
                return value.Value;

            return _right.Evaluate(call).Value;
        }
    }
}