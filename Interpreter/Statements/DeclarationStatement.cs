using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class DeclarationStatement : Statement
{
    private readonly bool _mask;
    private readonly bool _mutable;
    private readonly VariableScope _scope;

    internal List<Declaration> Declarations { get; } = new();

    internal DeclarationStatement(bool mask, bool mutable, VariableScope scope)
    {
        _mask = mask;
        _mutable = mutable;
        _scope = scope;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        foreach (var declaration in Declarations)
        {
            try
            {
                var value = declaration.Expression?.Evaluate(call).Value ?? Null.Value;

                declaration.Identifier.Define(value, call, _mask, _mutable, _scope);
            }
            catch (Throw exception)
            {
                return new[] { exception };
            }
        }

        return Enumerable.Empty<IResult>();
    }

    internal sealed record Declaration(IIdentifier Identifier, IExpression? Expression);
}