using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.SubExpressions;
using Bloc.Values;
using Void = Bloc.Values.Void;

namespace Bloc.Expressions;

internal sealed class FuncLiteral : IExpression
{
    private readonly FunctionType _type;
    private readonly CaptureMode _mode;
    private readonly Parameter? _argsContainer;
    private readonly Parameter? _kwargsContainer;
    private readonly List<Parameter> _parameters;
    private readonly List<Statement> _statements;

    internal FuncLiteral(
        FunctionType type,
        CaptureMode mode,
        Parameter? argsContainer,
        Parameter? kwargsContainer,
        List<Parameter> parameters,
        List<Statement> statements)
    {
        _type = type;
        _mode = mode;
        _argsContainer = argsContainer;
        _kwargsContainer = kwargsContainer;
        _parameters = parameters;
        _statements = statements;
    }

    public IValue Evaluate(Call call)
    {
        var captures = _mode switch
        {
            CaptureMode.Value => call.ValueCapture(),
            CaptureMode.Reference => call.ReferenceCapture(),
            _ => new()
        };

        var argsContainer = _argsContainer is not null
            ? new Func.Parameter(_argsContainer.Name, null)
            : null;

        var kwargsContainer = _kwargsContainer is not null
            ? new Func.Parameter(_kwargsContainer.Name, null)
            : null;

        var parameters = new List<Func.Parameter>();

        foreach (var parameter in _parameters)
        {
            var value = parameter.Expression?.Evaluate(call).Value.GetOrCopy();

            if (value is Void)
                throw new Throw("'void' is not assignable");

            parameters.Add(new(parameter.Name, value));
        }

        return new Func(_type, _mode, captures, argsContainer, kwargsContainer, parameters, _statements);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_type, _mode, _parameters.Count, _statements.Count);
    }

    public override bool Equals(object other)
    {
        return other is FuncLiteral literal &&
            _type == literal._type &&
            _mode == literal._mode &&
            _parameters.SequenceEqual(literal._parameters) &&
            _statements.SequenceEqual(literal._statements);
    }
}