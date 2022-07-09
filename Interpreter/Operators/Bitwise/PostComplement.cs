using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using Bloc.Variables;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Operators.Bitwise
{
    internal class PostComplement : IExpression
    {
        private readonly IExpression _operand;

        internal PostComplement(IExpression operand)
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
            if (value is not Variables.Variable variable)
                throw new Throw("The operand of an increment must be a variable");

            if (value.Value.Is(out Number? number))
            {
                variable.Value = new Number(~number!.ToInt());

                return new Number(number.ToInt());
            }

            if (value.Value.Is(out Values.Type? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                variable.Value = new Values.Type(types);

                return type!;
            }

            throw new Throw($"Cannot apply operator '~~' on type {variable.GetType().ToString().ToLower()}");
        }
    }
}