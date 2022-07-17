using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Character : IExpression
    {
        private readonly IExpression _operand;

        internal Character(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (!value.Is(out Number? num))
                throw new Throw($"Cannot apply operator 'chr' on type {value.GetType().ToString().ToLower()}");

            return new String(((char)num!.ToInt()).ToString());
        }
    }
}