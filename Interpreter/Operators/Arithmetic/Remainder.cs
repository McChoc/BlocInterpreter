using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Remainder : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Remainder(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IValue Evaluate(Call call)
        {
            var left = _left.Evaluate(call).Value;
            var right = _right.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(left, right, Operation, call);
        }

        internal static Value Operation(Value a, Value b)
        {
            return (a, b) switch
            {
                (IScalar left, IScalar right) => RemScalars(left, right),

                _ => throw new Throw($"Cannot apply operator '%' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}"),
            };
        }

        private static Number RemScalars(IScalar left, IScalar right)
        {
            return new Number(left.GetDouble() % right.GetDouble());
        }
    }
}