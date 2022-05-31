using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Operators.Boolean
{
    internal class PostNegation : IExpression
    {
        private readonly IExpression _operand;

        internal PostNegation(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Variable variable)
                throw new Throw("The operand of an increment must be a variable");

            if (!variable.Value.Is(out Bool? @bool))
                throw new Throw($"Cannot apply operator '!!' on type {variable.Type.ToString().ToLower()}");

            variable.Value = new Bool(!@bool!.Value);

            return @bool;
        }
    }
}
