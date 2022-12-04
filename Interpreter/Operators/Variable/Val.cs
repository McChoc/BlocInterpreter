using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Val : IExpression
    {
        private readonly IExpression _operand;

        internal Val(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            if (value is not Reference)
                throw new Throw("The 'val' operator can only be used on references");

            return ReferenceUtil.Dereference(value, call.Engine.HopLimit);
        }
    }
}