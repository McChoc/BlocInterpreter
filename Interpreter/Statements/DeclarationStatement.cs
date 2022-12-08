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
    internal abstract class DeclarationStatement : Statement, IEnumerable
    {
        protected abstract bool Mask { get; }
        protected abstract bool Mutable { get; }

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

        private void Define(IValue identifier, Value value, Call call)
        {
            if (identifier is UnresolvedPointer pointer)
            {
                pointer.Define(Mask, Mutable, value, call);
            }
            else if (identifier.Value is Tuple leftTuple)
            {
                if (value is not Tuple rightTuple)
                {
                    foreach (var id in leftTuple.Variables)
                        Define(id, value, call);
                }
                else
                {
                    if (leftTuple.Variables.Count != rightTuple.Variables.Count)
                        throw new Throw("Miss match number of elements in tuples.");

                    foreach (var (id, val) in leftTuple.Variables.Zip(rightTuple.Variables, (i, v) => (i, v.Value)))
                        Define(id, val, call);
                }
            }
            else
            {
                throw new Throw("The left part of an assignement must be an identifier");
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Mutable, Definitions.Count);
        }

        public override bool Equals(object other)
        {
            return other is DeclarationStatement statement &&
                Label == statement.Label &&
                Mutable == statement.Mutable &&
                Definitions.SequenceEqual(statement.Definitions);
        }

        internal void Add(IExpression identifier, IExpression? value) => Definitions.Add((identifier, value));

        IEnumerator IEnumerable.GetEnumerator() => Definitions.GetEnumerator();
    }
}