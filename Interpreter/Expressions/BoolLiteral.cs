using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed record BoolLiteral : IExpression
    {
        private readonly bool _bool;

        internal BoolLiteral(bool @bool) => _bool = @bool;

        public IPointer Evaluate(Call _) => _bool ? Bool.True : Bool.False;
    }
}