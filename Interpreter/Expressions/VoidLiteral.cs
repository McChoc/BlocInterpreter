using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed record VoidLiteral : IExpression
    {
        public IPointer Evaluate(Call _) => Void.Value;
    }
}