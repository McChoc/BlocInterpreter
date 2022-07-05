using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Statements
{
    internal class DefStatement : Statement
    {
        internal List<(IExpression, IExpression?)> Definitions { get; set; } = new();

        internal override Result? Execute(Call call)
        {
            foreach (var definition in Definitions)
            {
                try
                {
                    var value = definition.Item2?.Evaluate(call) ?? Null.Value;

                    var identifier = definition.Item1.Evaluate(call);

                    Define(identifier, value.Copy(), call);
                }
                catch (Result result)
                {
                    return result;
                }
            }

            return null;
        }

        private void Define(IValue identifier, Value value, Call call)
        {
            if (identifier is StackVariable stackVariable)
            {
                throw new Throw($"Variable '{stackVariable.Name}' was already defined in scope");
            }

            if (identifier is UndefinedVariable undefined)
            {
                value.Assign();

                call.TryAdd(new StackVariable(value, undefined.Name, call.Scopes[^1]));
            }
            else if (identifier.Value is Tuple leftTuple)
            {
                if (!value.Is(out Tuple? tuple))
                {
                    foreach (var id in leftTuple.Values)
                        Define(id, value, call);
                }
                else
                {
                    if (leftTuple.Values.Count != tuple!.Values.Count)
                        throw new Throw("Miss match number of elements in tuples.");

                    foreach (var (id, val) in leftTuple.Values.Zip(tuple.Values, (i, v) => (i, v.Value)))
                        Define(id, val, call);
                }
            }
            else
            {
                throw new Throw("The left part of an assignement must be a variable");
            }
        }
    }
}