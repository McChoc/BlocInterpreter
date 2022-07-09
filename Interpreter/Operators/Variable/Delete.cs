using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;
using System.Collections.Generic;
using System.Linq;

namespace Bloc.Operators.Variable
{
    internal class Delete : IExpression
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

        private Value Undefine(IValue val)
        {
            if (val is StackVariable or HeapVariable)
            {
                var value = val.Value;
                val.Value.Destroy();
                return value;
            }

            if (val is Tuple tpl)
            {
                var values = new List<Value>();

                foreach (var item in tpl.Values)
                    values.Add(Undefine(item));

                return new Tuple(values.ToList<IValue>());
            }

            throw new Throw("You can only delete a variable");
        }
    }
}