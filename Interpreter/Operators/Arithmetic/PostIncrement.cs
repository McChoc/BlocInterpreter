using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record PostIncrement : IExpression
    {
        private readonly IExpression _operand;

        internal PostIncrement(IExpression operand)
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
            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator '++' on type {value.GetType().ToString().ToLower()}");

            var original = new Number(scalar.GetDouble());
            var incremented = new Number(scalar.GetDouble() + 1);

            return (original, incremented);
        }
    }
}