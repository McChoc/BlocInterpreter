using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Let : IExpression
    {
        private readonly IExpression _operand;

        internal Let(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return Define(identifier, call);
        }

        private IPointer Define(IPointer identifier, Call call)
        {
            if (identifier is Pointer pointer)
                return pointer.Define(Null.Value, call);

            if (identifier.Value is Tuple tuple)
            {
                var values = new List<IPointer>(tuple.Values.Count);

                foreach (var id in tuple.Values)
                    values.Add(Define(id, call));

                return new Tuple(values);
            }

            throw new Throw("The operand must be an identifier");
        }
    }
}