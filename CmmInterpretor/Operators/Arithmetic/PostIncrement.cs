using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Operators.Arithmetic
{
    internal class PostIncrement : IExpression
    {
        private readonly IExpression _operand;

        internal PostIncrement(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return TupleUtil.RecursivelyCall(value, Operation);
        }

        private static IValue Operation(IValue value)
        {
            if (value is not Variable variable)
                throw new Throw("The operand of an increment must be a variable");

            if (!variable.Value.Is(out Number? number))
                throw new Throw($"Cannot apply operator '++' on type {variable.Type.ToString().ToLower()}");

            variable.Value = new Number(number!.Value + 1);

            return number;
        }
    }
}