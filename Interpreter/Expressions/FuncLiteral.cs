using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Statements;
using Bloc.Values;

namespace Bloc.Expressions
{
    internal sealed class FuncLiteral : IExpression
    {
        private readonly FunctionType _type;
        private readonly CaptureMode _mode;
        private readonly List<Parameter> _parameters;
        private readonly List<Statement> _statements;

        internal FuncLiteral(FunctionType type, CaptureMode mode, List<Parameter> parameters, List<Statement> statements)
        {
            _type = type;
            _mode = mode;
            _parameters = parameters;
            _statements = statements;
        }

        public IPointer Evaluate(Call call)
        {
            var captures = _mode switch
            {
                CaptureMode.Value => call.ValueCapture(),
                CaptureMode.Reference => call.ReferenceCapture(),
                _ => null
            };

            var parameters = _parameters
                .Select(x => new Func.Parameter(x.Name, x.Expression.Evaluate(call).Value))
                .ToList();

            return new Func(_type, _mode, captures, parameters, _statements);
        }

        public override bool Equals(object obj)
        {
            if (obj is not FuncLiteral func)
                return false;

            if (_type != func._type)
                return false;

            if (_mode != func._mode)
                return false;

            if (!_parameters.SequenceEqual(func._parameters))
                return false;

            if (!_statements.SequenceEqual(func._statements))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, _mode);
        }

        internal sealed class Parameter
        {
            internal string Name { get; }
            internal IExpression Expression { get; }

            internal Parameter(string name, IExpression expression)
            {
                Name = name;
                Expression = expression;
            }
        }
    }
}