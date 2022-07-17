using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Length : IExpression
    {
        private readonly IExpression _operand;

        internal Length(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value.Is(out Array? arr))
                return new Number(arr!.Values.Count);

            if (value.Is(out String? str))
                return new Number(str!.Value.Length);

            throw new Throw($"Cannot apply operator 'len' type {value.GetType().ToString().ToLower()}");
        }
    }
}