using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Length : IExpression
    {
        private readonly IExpression _operand;

        internal Length(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is Array array)
                return new Number(array.Variables.Count);

            if (value is String @string)
                return new Number(@string.Value.Length);

            throw new Throw($"Cannot apply operator 'len' type {value.GetType().ToString().ToLower()}");
        }
    }
}