using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Variables;
using CmmInterpretor.Values;

namespace CmmInterpretor.Operators.Reference
{
    public class Allocation : IExpression
    {
        private readonly IExpression _operand;

        public Allocation(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            value = value!.Copy();
            value.Assign();

            var variable = new HeapVariable(value.Value);
            var reference = new Values.Reference(variable);

            variable.References.Add(reference);

            return reference;
        }
    }
}
