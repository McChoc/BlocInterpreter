﻿using System;
using System.Collections.Generic;
using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using ValueType = CmmInterpretor.Values.ValueType;

namespace CmmInterpretor.Operators.Bitwise
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
            if (value is not Variable variable)
                throw new Throw("The operand of an increment must be a variable");

            if (value.Is(out Number? number))
            {
                variable.Value = new Number(~number!.ToInt());

                return new Number(number.ToInt());
            }

            if (value.Is(out TypeCollection? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                variable.Value = new TypeCollection(types);

                return type!;
            }

            throw new Throw($"Cannot apply operator '~~' on type {variable.Type.ToString().ToLower()}");
        }
    }
}