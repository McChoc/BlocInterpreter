using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal sealed record Global : IExpression
    {
        private readonly IExpression _operand;

        internal Global(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return GetGlobal(identifier, call);
        }

        private IValue GetGlobal(IValue identifier, Call call)
        {
            if (identifier is UnresolvedPointer pointer)
            {
                if (pointer.Global is null)
                    throw new Throw($"Variable {pointer.Name} was not defined in global scope");

                return new VariablePointer(pointer.Global);
            }

            if (identifier.Value is Tuple tuple)
            {
                var variables = new List<IVariable>(tuple.Variables.Count);

                foreach (var id in tuple.Variables)
                {
                    var value = GetGlobal(id, call);

                    if (value is IVariable variable)
                        variables.Add(variable);
                    else
                        variables.Add(new TupleVariable(value.Value));
                }

                return new Tuple(variables);
            }

            throw new Throw("The operand must be an identifier");
        }
    }
}