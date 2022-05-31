using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class VoidLiteral : IExpression
    {
        public IValue Evaluate(Call _) => Void.Value;
    }
}
