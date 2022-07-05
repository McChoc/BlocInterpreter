using Bloc.Memory;
using Bloc.Expressions;
using Bloc.Results;
using Bloc.Variables;
using Bloc.Values;

namespace Bloc.Operators.Reference
{
    internal class Reference : IExpression
    {
        private readonly IExpression _operand;

        internal Reference(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Variable variable)
                throw new Throw("The right part of a reference must be a variable");

            var reference = new Values.Reference(variable);
            variable.References.Add(reference);
            return reference;
        }
    }
}
