using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record PreDecrement : IExpression
    {
        private readonly IExpression _operand;

        internal PreDecrement(IExpression operand)
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
            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator '--' on type {value.GetType().ToString().ToLower()}");

            var number = new Number(scalar.GetDouble() - 1);

            return (number, number);
        }
    }
}