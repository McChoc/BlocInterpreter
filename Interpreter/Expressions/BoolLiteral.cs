using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class BoolLiteral : IExpression
    {
        private readonly bool _bool;

        internal BoolLiteral(bool @bool)
        {
            _bool = @bool;
        }

        public IValue Evaluate(Call _) => _bool ? Bool.True : Bool.False;
    }
}