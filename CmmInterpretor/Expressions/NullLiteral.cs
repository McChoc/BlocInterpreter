using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class NullLiteral : IExpression
    {
        public IValue Evaluate(Call _) => Null.Value;
    }
}
