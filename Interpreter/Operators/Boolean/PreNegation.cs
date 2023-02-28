using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record PreNegation : IExpression
    {
        private readonly IExpression _operand;

        internal PreNegation(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return AdjustmentUtil.Adjust(value, Adjustment, call);
        }

        private static (Value, Value) Adjustment(Value value)
        {
            var @bool = Bool.ImplicitCast(value);

            @bool = new Bool(!@bool.Value);

            return (@bool, @bool);
        }
    }
}