using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Nullable : IExpression
    {
        private readonly IExpression _operand;

        internal Nullable(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            if (value.Is(out Type? type))
            {
                var types = new HashSet<ValueType>
                {
                    ValueType.Null
                };

                foreach (var t in type!.Value)
                    types.Add(t);

                return new Type(types);
            }

            throw new Throw($"Cannot apply operator '?' on type {value.GetType().ToString().ToLower()}");
        }
    }
}