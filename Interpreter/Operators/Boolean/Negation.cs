using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Boolean
{
    internal class Negation : IExpression
    {
        private readonly IExpression _operand;

        internal Negation(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Value.Is(out Bool? @bool))
                return new Bool(!@bool!.Value);

            throw new Throw($"Cannot apply operator '!' on type {value.GetType().ToString().ToLower()}");
        }
    }
}