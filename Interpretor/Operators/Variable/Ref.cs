using Bloc.Memory;
using Bloc.Expressions;
using Bloc.Results;
using Bloc.Variables;
using Bloc.Values;

namespace Bloc.Operators.Variable
{
    internal class Ref : IExpression
    {
        private readonly IExpression _operand;

        internal Ref(IExpression operand) => _operand = operand;

        public IValue Evaluate(Call call)
        {
            var value = _operand.Evaluate(call);

            if (value is not Variables.Variable variable)
                throw new Throw("The right part of a reference must be a variable");

            if (variable is UndefinedVariable undefined)
                throw new Throw($"Variable {undefined.Name} was not defined in scope");

            var reference = new Values.Reference(variable);
            variable.References.Add(reference);
            return reference;
        }
    }
}
