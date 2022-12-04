using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Plus : IExpression
    {
        private readonly IExpression _operand;

        internal Plus(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(value, Operation, call);
        }

        private static Value Operation(Value value)
        {
            if (value is IScalar scalar)
                return new Number(scalar.GetDouble());

            throw new Throw($"Cannot apply operator '+' on type {value.GetType().ToString().ToLower()}");
        }
    }
}