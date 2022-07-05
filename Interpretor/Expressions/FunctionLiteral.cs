using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Statements;
using Bloc.Values;

namespace Bloc.Expressions
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