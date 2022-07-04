using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Character
{
    internal class Ordinal : IExpression
    {
        private readonly IExpression _operand;

        internal Ordinal(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (!value.Is(out String? str))
                throw new Throw($"Cannot apply operator 'ord' on type {value!.Type.ToString().ToLower()}");

            if (str!.Value.Length != 1)
                throw new Throw("The string must contain exactly one character");

            return new Number(str.Value[0]);
        }
    }
}