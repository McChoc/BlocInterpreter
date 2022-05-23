using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Operators.Bitwise
{
    public class Complement : IExpression
    {
        private readonly IExpression _operand;

        public Complement(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return TupleUtil.RecursivelyCall(value, Operation);
        }

        private static IValue Operation(IValue value)
        {
            if (value.Is(out Number? number))
                return new Number(~number!.ToInt());

            if (value.Is(out TypeCollection? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                return new TypeCollection(types);
            }

            throw new Throw($"Cannot apply operator '~' on type {value.Type.ToString().ToLower()}");
        }
    }
}
