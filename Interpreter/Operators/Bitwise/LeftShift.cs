using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record LeftShift : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal LeftShift(IExpression left, IExpression right)
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
            if (a is IScalar left && b is IScalar right)
                return new Number(left.GetInt() << right.GetInt());

            throw new Throw($"Cannot apply operator '<<' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}");
        }
    }
}