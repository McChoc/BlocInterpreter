using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed record Delete : IExpression
    {
        private readonly IExpression _operand;

        internal Delete(IExpression operand)
        {
            _operand = operand;
        }

        public IValue Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return Undefine(identifier);
        }

        private Value Undefine(IValue identifier)
        {
            if (identifier is Pointer pointer)
                return pointer.Delete();

            if (identifier.Value is Tuple tuple)
            {
                var values = new List<Value>(tuple.Variables.Count);

                foreach (var item in tuple.Variables)
                    values.Add(Undefine(item));

                return new Tuple(values);
            }

            throw new Throw("You can only delete a variable");
        }
    }
}