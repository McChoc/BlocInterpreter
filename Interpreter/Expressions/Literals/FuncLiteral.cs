using System.Collections.Generic;
using System.Linq;
using Bloc.Funcs;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

[Record]
internal sealed partial class FuncLiteral : IExpression
{
    private readonly FuncType _type;
    private readonly CaptureMode _mode;
    private readonly INamedIdentifier? _packingParameterIdentifier;
    private readonly INamedIdentifier? _kwPackingParameterIdentifier;
    private readonly List<Parameter> _parameters;
    private readonly List<Statement> _statements;

    internal FuncLiteral(
        FuncType type,
        CaptureMode mode,
        INamedIdentifier? packingParameterIdentifier,
        INamedIdentifier? kwPackingParameterIdentifier,
        List<Parameter> parameters,
        List<Statement> statements)
    {
        _type = type;
        _mode = mode;
        _packingParameterIdentifier = packingParameterIdentifier;
        _kwPackingParameterIdentifier = kwPackingParameterIdentifier;
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

        var packingParameterName = _packingParameterIdentifier?.GetName(call);
        var kwPackingParameterName = _kwPackingParameterIdentifier?.GetName(call);

        var parameters = new List<Func.Parameter>();

        foreach (var parameter in _parameters)
        {
            var name = parameter.Identifier.GetName(call);

            if (parameters.Any(x => x.Name == name))
                throw new Throw("Duplicate parameter names");

            var value = parameter.Expression?.Evaluate(call).Value.GetOrCopy();

            if (value is Void)
                throw new Throw("'void' is not assignable");

            parameters.Add(new(name, value, parameter.Type));
        }

        return new Func(_type, _mode, call.Module.TopLevelScope, captures, packingParameterName, kwPackingParameterName, parameters, _statements);
    }

    internal sealed record Parameter(INamedIdentifier Identifier, IExpression? Expression, ParameterType Type);
}