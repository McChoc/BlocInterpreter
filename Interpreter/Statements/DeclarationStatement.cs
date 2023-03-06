using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Statements;

internal class DeclarationStatement : Statement
{
    private readonly bool _mask;
    private readonly bool _mutable;

    internal List<(IExpression Name, IExpression? Value)> Definitions { get; } = new();

    internal DeclarationStatement(bool mask, bool mutable)
    {
        _mask = mask;
        _mutable = mutable;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        foreach (var definition in Definitions)
        {
            try
            {
                var value = definition.Value?.Evaluate(call).Value ?? Null.Value;

                var identifier = definition.Name.Evaluate(call);

                VariableHelper.Define(identifier, value, call, _mask, _mutable);
            }
            catch (Throw exception)
            {
                return new[] { exception };
            }
        }

        return Enumerable.Empty<IResult>();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _mask, _mutable, Definitions.Count);
    }

    public override bool Equals(object other)
    {
        return other is DeclarationStatement statement &&
            Label == statement.Label &&
            _mask == statement._mask &&
            _mutable == statement._mutable &&
            Definitions.SequenceEqual(statement.Definitions);
    }
}