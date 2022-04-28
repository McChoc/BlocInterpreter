using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class UndefStatement : Statement
    {
        private readonly List<List<Token>> _undefinitions;

        public UndefStatement(List<List<Token>> undefinitions) => _undefinitions = undefinitions;

        public override IResult Execute(Call call)
        {
            foreach (List<Token> undefinition in _undefinitions)
            {
                if (Evaluator.Evaluate(undefinition, call) is not Pointer ptr)
                    throw new SyntaxError("Only a variable can be undefined.");

                ptr.Remove();
            }

            return new Void();
        }
    }
}
