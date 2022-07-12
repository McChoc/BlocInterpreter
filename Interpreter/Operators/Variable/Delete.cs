using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Delete : IExpression
    {
        private readonly IExpression _operand;

        internal Delete(IExpression operand)
        {
            _operand = operand;
        }

        public IPointer Evaluate(Call call)
        {
            var identifier = _operand.Evaluate(call);

            return Undefine(identifier);
        }

        private Value Undefine(IPointer value)
        {
            if (value is Pointer pointer)
                return pointer.Delete();

            if (value is Tuple tuple)
            {
                var values = new List<IPointer>(tuple.Values.Count);

                foreach (var item in tuple.Values)
                    values.Add(Undefine(item));

                return new Tuple(values);
            }

            throw new Throw("You can only delete a variable");
        }
    }
}