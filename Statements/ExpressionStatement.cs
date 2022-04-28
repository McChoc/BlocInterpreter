using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ExpressionStatement : Statement
    {
        private readonly List<Token> _tokens;

        public ExpressionStatement(List<Token> tokens) => _tokens = tokens;

        public override IResult Execute(Call call)
        {
            return Evaluator.Evaluate(_tokens, call);
        }
    }
}
