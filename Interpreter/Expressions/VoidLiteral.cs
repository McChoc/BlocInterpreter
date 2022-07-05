using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class VoidLiteral : IExpression
    {
        public IValue Evaluate(Call _) => Void.Value;
    }
}