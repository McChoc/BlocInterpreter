using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record PostNegation : IExpression
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
            var @bool = Bool.ImplicitCast(value);

            return (@bool, new Bool(!@bool.Value));
        }
    }
}