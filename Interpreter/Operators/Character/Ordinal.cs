using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Ordinal : IExpression
    {
        private readonly IExpression _operand;

        internal Ordinal(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine).Value;

            if (!value.Is(out String? str))
                throw new Throw($"Cannot apply operator 'ord' on type {value!.GetType().ToString().ToLower()}");

            if (str!.Value.Length != 1)
                throw new Throw("The string must contain exactly one character");

            return new Number(str.Value[0]);
        }
    }
}