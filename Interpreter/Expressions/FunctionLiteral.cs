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
        private readonly CaptureMode _mode;
        private readonly List<string> _parameters;
        private readonly List<Statement> _statements;

        internal FunctionLiteral(bool async, CaptureMode mode, List<string> parameters, List<Statement> statements)
        {
            _async = async;
            _mode = mode;
            _parameters = parameters;
            _statements = statements;
        }

        public IPointer Evaluate(Call call)
        {
            var function = new Function
            {
                Async = _async,
                Mode = _mode,
                Names = _parameters,
                Code = _statements
            };

            if (_mode == CaptureMode.Value)
                function.Captures = call.ValueCapture();

            if (_mode == CaptureMode.Reference)
                function.Captures = call.ReferenceCapture();

            return function;
        }
    }
}