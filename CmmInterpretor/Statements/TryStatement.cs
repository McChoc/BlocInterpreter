using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class TryStatement : Statement
    {
        public List<Statement> Try { get; set; } = default!;
        public List<Statement> Catch { get; set; } = new();
        public List<Statement> Finally { get; set; } = new();

        public override Result? Execute(Call call)
        {
            var result = ExecuteBlock(Try, call);
            
            if (result is Throw)
            {
                result = null;

                var catchResult = ExecuteBlock(Catch, call);

                if (catchResult is not null)
                    return catchResult;
            }

            var finallyResult = ExecuteBlock(Finally, call);

            if (finallyResult is not null)
                return finallyResult;

            return result;
        }
    }
}
