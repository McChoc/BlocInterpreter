using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Expressions
{
    internal class TypeLiteral : IExpression
    {
        private readonly ValueType _type;

        internal TypeLiteral(ValueType type) => _type = type;

        public IValue Evaluate(Call _) => new TypeCollection(_type);
    }
}
