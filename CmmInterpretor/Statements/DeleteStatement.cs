using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Statements
{
    internal class DeleteStatement : Statement
    {
        private readonly IExpression _expression;

        internal DeleteStatement(IExpression expression) => _expression = expression;

        internal override Result? Execute(Call call)
        {
            try
            {
                Delete(_expression.Evaluate(call));
            }
            catch (Result result)
            {
                return result;
            }

            return null;
        }

        private void Delete(IValue val)
        {
            if (val is StackVariable or HeapVariable)
            {
                val.Destroy();
            }
            else if (val is Tuple tpl)
            {
                foreach (var item in tpl.Values)
                    Delete(item);
            }
            else
            {
                throw new Throw("You can only delete a variable");
            }
        }
    }
}
