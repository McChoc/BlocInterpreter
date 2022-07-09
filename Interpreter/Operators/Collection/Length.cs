using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Collection
{
    internal class Length : IExpression
    {
        private readonly IExpression _operand;

        internal Length(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value.Value.Is(out Array? arr))
                return new Number(arr!.Values.Count);

            if (value.Value.Is(out String? str))
                return new Number(str!.Value.Length);

            throw new Throw($"Cannot apply operator 'len' type {value!.GetType().ToString().ToLower()}");
        }
    }
}