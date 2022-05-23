using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Operators.Type
{
    public class Nullable : IExpression
    {
        private readonly IExpression _operand;

        public Nullable(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Is(out TypeCollection? type))
            {
                var types = new HashSet<ValueType>
                {
                    ValueType.Null
                };

                foreach (ValueType t in type!.Value)
                    types.Add(t);

                return new TypeCollection(types);
            }

            throw new Throw($"Cannot apply operator '?' on type {value.Type.ToString().ToLower()}");
        }
    }
}
