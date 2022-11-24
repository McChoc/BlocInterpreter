using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record ConstStatement : Statement, IEnumerable
    {
        internal List<(IExpression Name, IExpression? Value)> Definitions { get; set; } = new();

        internal override IEnumerable<Result> Execute(Call call)
        {
            foreach (var definition in Definitions)
            {
                try
                {
                    var value = definition.Value?.Evaluate(call) ?? Null.Value;

                    var identifier = definition.Name.Evaluate(call);

                    Define(identifier, value.Value.Copy(), call);
                }
                catch (Throw exception)
                {
                    return new[] { exception };
                }
            }

            return Enumerable.Empty<Result>();
        }

        private void Define(IPointer identifier, Value value, Call call)
        {
            if (identifier is Pointer pointer)
            {
                value.Assign();
                pointer.Define(value, call);
            }
            else if (identifier.Value is Tuple leftTuple)
            {
                if (value is not Tuple rightTuple)
                {
                    foreach (var id in leftTuple.Values)
                        Define(id, value, call);
                }
                else
                {
                    if (leftTuple.Values.Count != rightTuple.Values.Count)
                        throw new Throw("Miss match number of elements in tuples.");

                    foreach (var (id, val) in leftTuple.Values.Zip(rightTuple.Values, (i, v) => (i, v.Value)))
                        Define(id, val, call);
                }
            }
            else
            {
                throw new Throw("The left part of an assignement must be a variable");
            }
        }

        internal void Add(IExpression identifier, IExpression? value) => Definitions.Add((identifier, value));

        IEnumerator IEnumerable.GetEnumerator() => Definitions.GetEnumerator();
    }
}