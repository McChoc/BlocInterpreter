using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Operators
{
    internal sealed record Param : IExpression
    {
        private readonly IExpression _operand;

        internal Param(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return GetCapture(identifier, call);
        }

        private IValue GetCapture(IValue identifier, Call call)
        {
            if (identifier is UnresolvedPointer pointer)
            {
                if (pointer.Param is null)
                    throw new Throw($"Variable {pointer.Name} was not defined in parameters");

                return new VariablePointer(pointer.Param);
            }

            if (identifier.Value is Tuple tuple)
            {
                var variables = new List<IVariable>(tuple.Variables.Count);

                foreach (var id in tuple.Variables)
                {
                    var value = GetCapture(id, call);

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