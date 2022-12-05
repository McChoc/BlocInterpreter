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
            if (identifier is Pointer pointer)
            {
                var name = pointer switch
                {
                    UndefinedPointer undefined => undefined.Name,
                    VariablePointer { Variable: StackVariable var } => var.Name,
                    _ => throw new Throw("The operand must be an identifier")
                };

                if (!call.Engine.GlobalScope.Variables.TryGetValue(name, out var variable))
                    throw new Throw($"Variable {name} was not defined in global scope");

                return new VariablePointer(variable);
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