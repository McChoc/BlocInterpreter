using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Minus : IExpression
    {
        private readonly IExpression _operand;

        internal Minus(IExpression operand)
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
                return new Number(-number!.Value);

            throw new Throw($"Cannot apply operator '-' on type {value.Value.GetType().ToString().ToLower()}");
        }
    }
}