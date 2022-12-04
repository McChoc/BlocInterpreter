using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Next : IExpression
    {
        private readonly IExpression _operand;

        internal Next(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is Iter iter)
                return iter.Next();

            throw new Throw($"Cannot apply operator 'next' on type {value.GetType().ToString().ToLower()}");
        }
    }
}