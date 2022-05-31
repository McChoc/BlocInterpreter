using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Boolean
{
    internal class BooleanOr : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanOr(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var value = _left.Evaluate(call);

            if (!value.Is(out Bool? @bool))
                throw new Throw("Cannot implicitly convert to bool");

            if (@bool!.Value)
                return value.Value;

            return _right.Evaluate(call).Value;
        }
    }
}
