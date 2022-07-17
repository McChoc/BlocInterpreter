using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class PostComplement : IExpression
    {
        private readonly IExpression _operand;

        internal PostComplement(IExpression operand)
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
            if (!value.Is(out Number? number))
                throw new Throw($"Cannot apply operator '~~' on type {value.GetType().ToString().ToLower()}");

            return (number!, new Number(~number!.ToInt()));
        }
    }
}