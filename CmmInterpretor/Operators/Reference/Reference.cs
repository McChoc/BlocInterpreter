using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Variables;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Reference
{
    public class Reference : IExpression
    {
        private readonly IExpression _operand;

        public Reference(IExpression operand) => _operand = operand;

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
