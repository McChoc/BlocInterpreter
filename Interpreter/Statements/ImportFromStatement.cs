using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class ImportFromStatement : Statement
{
    private readonly VariableScope _scope;

    internal IExpression ModulePathExpression { get; }
    internal List<Import> Imports { get; } = new();

    internal ImportFromStatement(VariableScope scope, IExpression path)
    {
        _scope = scope;
        ModulePathExpression = path;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        Throw? exception = null;

        try
        {
            string path = ImportHelper.ResolveModulePath(ModulePathExpression, call);
            var module = ImportHelper.GetModule(path, call);

            foreach (var (nameIdentifier, aliasIdentifier) in Imports)
            {
                string name = nameIdentifier.GetName(call);
                string alias = aliasIdentifier?.GetName(call) ?? name;

                if (!module.Exports.TryGetValue(name, out var export))
                    throw new Throw($"Module '{path}' does not export {name}");

                call.Set(alias, export.Copy(true), false, true, _scope);
            }
        }
        catch (Throw t)
        {
            exception = t;
        }

        if (exception is not null)
            yield return exception;
    }

    internal sealed record Import(INamedIdentifier Name, INamedIdentifier? Alias);
}