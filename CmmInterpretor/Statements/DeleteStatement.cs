using CmmInterpretor.Memory;
using CmmInterpretor.Expressions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Statements
{
    public class DeleteStatement : Statement
    {
        private readonly IExpression _expression;

        public DeleteStatement(IExpression expression) => _expression = expression;

        public override Result? Execute(Call call)
        {
            try
            {
                return Delete(_expression.Evaluate(call));
            }
            catch (Result result)
            {
                return result;
            }
        }

        private Result? Delete(IValue val)
        {
            if (val is StackVariable or HeapVariable)
            {
                val.Destroy();
                return null;
            }

            if (val is Tuple tpl)
            {
                foreach (var item in tpl.Values)
                    Delete(item);

                return null;
            }

            return new Throw("You can only delete a variable");
        }
    }
}
