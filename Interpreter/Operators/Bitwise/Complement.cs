using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Operators.Bitwise
{
    internal class Complement : IExpression
    {
        private readonly IExpression _operand;

        internal Complement(IExpression operand)
        {
            _operand = operand;
        }

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

                foreach (ValueType t in Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                return new TypeCollection(types);
            }

            throw new Throw($"Cannot apply operator '~' on type {value.Type.ToString().ToLower()}");
        }
    }
}