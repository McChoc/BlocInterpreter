using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Character
{
    internal class Character : IExpression
    {
        private readonly IExpression _operand;

        internal Character(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (!value.Is(out Number? num))
                throw new Throw($"Cannot apply operator 'chr' on type {value.Type.ToString().ToLower()}");

            return new String(((char)num!.ToInt()).ToString());
        }
    }
}