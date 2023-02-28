using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Ordinal : IExpression
    {
        private readonly IExpression _operand;

        internal Ordinal(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not String @string)
                throw new Throw($"Cannot apply operator 'ord' on type {value!.GetType().ToString().ToLower()}");

            if (@string.Value.Length != 1)
                throw new Throw("The string must contain exactly one character");

            return new Number(@string.Value[0]);
        }
    }
}