using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class TryStatement : Statement
    {
        public Token @try;
        public Token @catch;
        public Token @finally;

        public override IResult Execute(Call call)
        {
            var result = ExecuteBlock((List<Statement>)@try.value, call);
            
            if (result is Throw)
            {
                result = new Void();

                if (@catch.type != TokenType.Empty)
                {
                    var catchResult = ExecuteBlock((List<Statement>)@catch.value, call);

                    if (catchResult is not IValue)
                        return catchResult;
                }
            }

            if (@finally.type != TokenType.Empty)
            {
                var finallyResult = ExecuteBlock((List<Statement>)@finally.value, call);

                if (finallyResult is not IValue)
                    return finallyResult;
            }

            return result;
        }
    }
}
