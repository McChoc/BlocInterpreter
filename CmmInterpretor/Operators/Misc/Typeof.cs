using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Misc
{
    internal class Typeof : IExpression
    {
        private readonly IExpression _operand;

        internal Typeof(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return new TypeCollection(value.Type);
        }
    }
}
