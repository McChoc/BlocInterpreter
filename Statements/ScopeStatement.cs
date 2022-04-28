using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class ScopeStatement : Statement
    {
        public Token body;

        public override IResult Execute(Call call)
        {
            return ExecuteBlock((List<Statement>)body.value, call);
        }
    }
}
