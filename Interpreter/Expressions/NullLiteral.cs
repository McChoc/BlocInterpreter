using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class NullLiteral : IExpression
    {
        public IPointer Evaluate(Call _) => Null.Value;
    }
}