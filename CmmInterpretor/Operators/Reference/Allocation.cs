using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Operators.Reference
{
    internal class Allocation : IExpression
    {
        private readonly IExpression _operand;

        internal Allocation(IExpression operand)
        {
            _operand = operand;
        }

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