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
            var value = _operand.Evaluate(call);

            return TupleUtil.RecursivelyCall(value, Operation);
        }

        private static IPointer Operation(IPointer value)
        {
            if (value.Value.Is(out Number? number))
                return new Number(~number!.ToInt());

            if (value.Value.Is(out Type? type))
            {
                var types = new HashSet<ValueType>();

                foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                    if (!type!.Value.Contains(t))
                        types.Add(t);

                return new Type(types);
            }

            throw new Throw($"Cannot apply operator '~' on type {value.Value.GetType().ToString().ToLower()}");
        }
    }
}