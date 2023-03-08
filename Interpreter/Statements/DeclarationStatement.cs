using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements;

internal class DeclarationStatement : Statement
{
    private readonly bool _mask;
    private readonly bool _mutable;

    internal List<Declaration> Declarations { get; } = new();

    internal DeclarationStatement(bool mask, bool mutable)
    {
        _mask = mask;
        _mutable = mutable;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        foreach (var declaration in Declarations)
        {
            try
            {
                var value = declaration.Expression?.Evaluate(call).Value ?? Null.Value;

                declaration.Identifier.Define(value, call, _mask, _mutable);
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
        return HashCode.Combine(Label, _mask, _mutable, Declarations.Count);
    }

    public override bool Equals(object other)
    {
        return other is DeclarationStatement statement &&
            Label == statement.Label &&
            _mask == statement._mask &&
            _mutable == statement._mutable &&
            Declarations.SequenceEqual(statement.Declarations);
    }

    internal sealed record Declaration(IIdentifier Identifier, IExpression? Expression);
}