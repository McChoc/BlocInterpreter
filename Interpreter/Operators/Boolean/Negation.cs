using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Negation : IExpression
    {
        private readonly IExpression _operand;

        internal Negation(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(value, Operation, call);
        }

        private static Value Operation(Value value)
        {
            if (value.Is(out Bool? @bool))
                return new Bool(!@bool!.Value);

            throw new Throw($"Cannot apply operator '!' on type {value.GetType().ToString().ToLower()}");
        }
    }
}