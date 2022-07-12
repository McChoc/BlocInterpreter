using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Statements;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal class FunctionLiteral : IExpression
    {
        private readonly bool _async;
        private readonly List<string> _parameters;
        private readonly List<Statement> _statements;

        internal FunctionLiteral(bool async, List<string> parameters, List<Statement> statements)
        {
            _async = async;
            _parameters = parameters;
            _statements = statements;
        }

        public IPointer Evaluate(Call call)
        {
            return new Function
            {
                Async = _async,
                Names = _parameters,
                Code = _statements,
                Captures = call.Capture()
            };
        }
    }
}