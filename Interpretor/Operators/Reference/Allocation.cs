using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators.Reference
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