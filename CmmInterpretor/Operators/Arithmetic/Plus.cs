using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using CmmInterpretor.Utils;

namespace CmmInterpretor.Operators.Arithmetic
{
    internal class Plus : IExpression
    {
        private readonly IExpression _operand;

        internal Plus(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return TupleUtil.RecursivelyCall(value, Operation);
        }

        private static IValue Operation(IValue value)
        {
            if (value.Is(out Number? number))
                return new Number(+number!.Value);

            throw new Throw($"Cannot apply operator '+' on type {value.Type.ToString().ToLower()}");
        }
    }
}
