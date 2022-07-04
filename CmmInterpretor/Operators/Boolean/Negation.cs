using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Boolean
{
    internal class Negation : IExpression
    {
        private readonly IExpression _operand;

        internal Negation(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Is(out Bool? @bool))
                return new Bool(!@bool!.Value);

            throw new Throw($"Cannot apply operator '!' on type {value.Type.ToString().ToLower()}");
        }
    }
}