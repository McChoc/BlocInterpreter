using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class DeleteStatement : Statement
    {
        private readonly List<Token> _tokens;

        public DeleteStatement(List<Token> token) => _tokens = token;

        public override IResult Execute(Call call)
        {
            var result = Evaluator.Evaluate(_tokens, call);

            if (result is not IValue val)
                return result;

            Delete(val);

            return Void.Value;
        }

        private void Delete(IValue val)
        {
            if (val is Variable var)
            {
                var.Destroy();
            }
            else if (val is Tuple tpl)
            {
                foreach (var item in tpl.Values)
                    Delete(item);
            }
        }
    }
}
