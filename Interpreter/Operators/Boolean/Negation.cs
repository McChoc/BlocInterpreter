using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Negation : IExpression
    {
        private readonly IExpression _operand;

        internal Negation(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call).Value;

            return OperatorUtil.RecursivelyCall(value, Operation, call);
        }

        private static Value Operation(Value value)
        {
            var @bool = Bool.ImplicitCast(value);

            return new Bool(!@bool.Value);
        }
    }
}