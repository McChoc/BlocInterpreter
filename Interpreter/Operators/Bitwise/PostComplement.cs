using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record PostComplement : IExpression
    {
        private readonly IExpression _operand;

        internal PostComplement(IExpression operand)
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
            return value switch
            {
                IScalar scalar  => ComplementScalar(scalar),
                Type type       => ComplementType(type),

                _ => throw new Throw($"Cannot apply operator '~~' on type {value.GetType().ToString().ToLower()}")
            };
        }

        private static (Number, Number) ComplementScalar(IScalar scalar)
        {
            var original = new Number(scalar.GetInt());
            var modified = new Number(~scalar.GetInt());

            return (original, modified);
        }

        private static (Type, Type) ComplementType(Type type)
        {
            var types = new HashSet<ValueType>();

            foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                if (!type.Value.Contains(t))
                    types.Add(t);

            return (type, new Type(types));
        }
    }
}