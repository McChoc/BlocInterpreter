using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Plus : IExpression
    {
        private readonly IExpression _operand;

        internal Plus(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(value, Operation, call);
        }

        private static Value Operation(Value value)
        {
            if (value.Is(out Number? number))
                return new Number(+number!.Value);

            throw new Throw($"Cannot apply operator '+' on type {value.GetType().ToString().ToLower()}");
        }
    }
}