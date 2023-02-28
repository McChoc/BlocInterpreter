using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Complement : IExpression
    {
        private readonly IExpression _operand;

        internal Complement(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(value, Operation, call);
        }

        internal static Value Operation(Value value)
        {
            return value switch
            {
                IScalar scalar  => ComplementScalar(scalar),
                Type type       => ComplementType(type),

                _ => throw new Throw($"Cannot apply operator '~' on type {value.GetType().ToString().ToLower()}")
            };
        }

        private static Number ComplementScalar(IScalar scalar)
        {
            return new Number(~scalar.GetInt());
        }

        private static Type ComplementType(Type type)
        {
            var types = new HashSet<ValueType>();

            foreach (ValueType t in System.Enum.GetValues(typeof(ValueType)))
                if (!type.Value.Contains(t))
                    types.Add(t);

            return new Type(types);
        }
    }
}