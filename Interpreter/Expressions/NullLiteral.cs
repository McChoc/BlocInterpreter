using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class NullLiteral : IExpression
    {
        public IValue Evaluate(Call _) => Null.Value;
    }
}