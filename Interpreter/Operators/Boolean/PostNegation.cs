using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class PostNegation : IExpression
    {
        private readonly IExpression _operand;

        internal PostNegation(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Pointer pointer)
                throw new Throw("The operand of an increment must be a variable");

            if (!pointer.Get().Is(out Bool? @bool))
                throw new Throw($"Cannot apply operator '!!' on type {pointer.Get().GetType().ToString().ToLower()}");

            pointer.Set(new Bool(!@bool!.Value));

            return @bool;
        }
    }
}