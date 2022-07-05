using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;
using System.Collections.Generic;

namespace Bloc.Operators.Variable
{
    internal class Let : IExpression
    {
        private readonly IExpression _operand;

        internal Let(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return Define(identifier, call);
        }

        private IValue Define(IValue identifier, Call call)
        {
            if (identifier is StackVariable stackVariable)
                throw new Throw($"Variable '{stackVariable.Name}' was already defined in scope");

            if (identifier is UndefinedVariable undefined)
            {
                var variable = new StackVariable(Null.Value, undefined.Name, call.Scopes[^1]);
                call.TryAdd(variable);
                return variable;
            }
            
            if (identifier.Value is Tuple tuple)
            {
                var values = new List<IValue>();

                foreach (var id in tuple.Values)
                    values.Add(Define(id, call));

                return new Tuple(values);
            }

            throw new Throw("The operand must be an identifier");
        }
    }
}