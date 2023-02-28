using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record ValVal : IExpression
    {
        private readonly IExpression _operand;

        internal ValVal(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            return ReferenceUtil.TrueValue(value.Value, call.Engine.HopLimit);
        }
    }
}