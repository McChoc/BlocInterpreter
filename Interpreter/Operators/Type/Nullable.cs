using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Type
{
    internal class Nullable : IExpression
    {
        private readonly IExpression _operand;

        internal Nullable(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Is(out TypeCollection? type))
            {
                var types = new HashSet<ValueType>
                {
                    ValueType.Null
                };

                foreach (var t in type!.Value)
                    types.Add(t);

                return new TypeCollection(types);
            }

            throw new Throw($"Cannot apply operator '?' on type {value.Type.ToString().ToLower()}");
        }
    }
}