using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Character : IExpression
    {
        private readonly IExpression _operand;

        internal Character(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator 'chr' on type {value.GetType().ToString().ToLower()}");

            return new String(((char)scalar.GetInt()).ToString());
        }
    }
}