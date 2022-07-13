using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class PostIncrement : IExpression
    {
        private readonly IExpression _operand;

        internal PostIncrement(IExpression operand)
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
            if (value is not Pointer pointer)
                throw new Throw("The operand of an increment must be a variable");

            if (!pointer.Get().Is(out Number? number))
                throw new Throw($"Cannot apply operator '++' on type {pointer.Get().GetType().ToString().ToLower()}");

            pointer.Set(new Number(number!.Value + 1));

            return number;
        }
    }
}