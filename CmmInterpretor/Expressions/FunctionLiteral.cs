using CmmInterpretor.Memory;
using CmmInterpretor.Statements;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Expressions
{
    internal class FunctionLiteral : IExpression
    {
        private readonly bool async;
        private readonly List<string> parameters;
        private readonly List<Statement> statements;

        internal FunctionLiteral(bool async, List<string> parameters, List<Statement> statements)
        {
            this.async = async;
            this.parameters = parameters;
            this.statements = statements;
        }

        public IValue Evaluate(Call call)
        {
            return new Function
            {
                Async = async,
                Captures = call.Capture(),
                Names = parameters,
                Code = statements
            };
        }
    }
}
