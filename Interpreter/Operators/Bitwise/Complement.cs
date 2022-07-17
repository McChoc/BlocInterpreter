using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Complement : IExpression
    {
        private readonly IExpression _operand;

        internal Complement(IExpression operand)
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
            if (value.Is(out Number? number))
                return new Number(~number!.ToInt());

            if (value.Is(out Type? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                return new Type(types);
            }

            throw new Throw($"Cannot apply operator '~' on type {value.GetType().ToString().ToLower()}");
        }
    }
}