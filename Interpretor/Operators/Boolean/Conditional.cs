using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Operators.Boolean
{
    internal class Conditional : IExpression
    {
        private readonly IExpression _alternative;
        private readonly IExpression _condition;
        private readonly IExpression _consequent;

        internal Conditional(IExpression condition, IExpression consequent, IExpression alternative)
        {
            _condition = condition;
            _consequent = consequent;
            _alternative = alternative;
        }

        public IValue Evaluate(Call call)
        {
            var value = _condition.Evaluate(call);

            var @bool = value.Implicit<Bool>();

            return @bool!.Value ? _consequent.Evaluate(call) : _alternative.Evaluate(call);
        }
    }
}