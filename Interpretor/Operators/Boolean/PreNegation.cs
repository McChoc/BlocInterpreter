using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators.Boolean
{
    internal class PreNegation : IExpression
    {
        private readonly IExpression _operand;

        internal PreNegation(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Variables.Variable variable)
                throw new Throw("The operand of an increment must be a variable");

            if (!variable.Value.Is(out Bool? @bool))
                throw new Throw($"Cannot apply operator '!!' on type {variable.Type.ToString().ToLower()}");

            return variable.Value = new Bool(!@bool!.Value);
        }
    }
}