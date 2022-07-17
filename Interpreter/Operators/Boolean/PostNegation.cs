using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class PostNegation : IExpression
    {
        private readonly IExpression _operand;

        internal PostNegation(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return AdjustmentUtil.Adjust(value, Adjustment, call);
        }

        private static (Value, Value) Adjustment(Value value)
        {
            if (!value.Is(out Bool? @bool))
                throw new Throw($"Cannot apply operator '!!' on type {value.GetType().ToString().ToLower()}");

            return (@bool!, new Bool(!@bool!.Value));
        }
    }
}