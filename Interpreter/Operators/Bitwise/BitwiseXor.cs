using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record BitwiseXor : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BitwiseXor(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value a, Value b)
        {
            return (a, b) switch
            {
                (IScalar left, IScalar right)   => XorScalars(left, right),
                (Type left, Type right)         => XorTypes(left, right),

                _ => throw new Throw($"Cannot apply operator '^' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}"),
            };
        }

        private static Number XorScalars(IScalar left, IScalar right)
        {
            return new Number(left.GetInt() ^ right.GetInt());
        }

        private static Type XorTypes(Type left, Type right)
        {
            var types = new HashSet<ValueType>();

            foreach (ValueType type in System.Enum.GetValues(typeof(ValueType)))
                if (left.Value.Contains(type) != right.Value.Contains(type))
                    types.Add(type);

            return new Type(types);
        }
    }
}