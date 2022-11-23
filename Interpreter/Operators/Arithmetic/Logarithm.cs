using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;
using static System.Math;

namespace Bloc.Operators
{
    internal sealed record Logarithm : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal Logarithm(IExpression left, IExpression right)
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
            if (a is IScalar left && b is IScalar right)
                return new Number(Log(left.GetDouble(), right.GetDouble()));

            throw new Throw($"Cannot apply operator '%%' on operands of types {a.GetType().ToString().ToLower()} and {b.GetType().ToString().ToLower()}");
        }
    }
}