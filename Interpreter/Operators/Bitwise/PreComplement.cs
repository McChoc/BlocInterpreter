using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class PreComplement : IExpression
    {
        private readonly IExpression _operand;

        internal PreComplement(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return TupleUtil.RecursivelyCall(value, Operation);
        }

        private static IPointer Operation(IPointer value)
        {
            if (value is not Pointer pointer)
                throw new Throw("The operand of an increment must be a variable");

            if (pointer.Get().Is(out Number? number))
                return pointer.Set(new Number(~number!.ToInt()));

            if (pointer.Get().Is(out Type? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                return pointer.Set(new Type(types));
            }

            throw new Throw($"Cannot apply operator '~~' on type {pointer.Get().GetType().ToString().ToLower()}");
        }
    }
}