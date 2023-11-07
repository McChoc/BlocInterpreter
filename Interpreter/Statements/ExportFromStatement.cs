using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Statements;

[Record]
internal sealed partial class ExportFromStatement : Statement
{
    internal IExpression ModulePathExpression { get; }
    internal List<Export> Exports { get; } = new();

    internal ExportFromStatement(IExpression path)
    {
        ModulePathExpression = path;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        Throw? exception = null;

        try
        {
            string path = ImportHelper.ResolveModulePath(ModulePathExpression, call);
            var module = ImportHelper.GetModule(path, call);

            foreach (var (nameIdentifier, aliasIdentifier) in Exports)
            {
                string name = nameIdentifier.GetName(call);
                string alias = aliasIdentifier?.GetName(call) ?? name;

                if (!module.Exports.TryGetValue(name, out var export))
                    throw new Throw($"Module '{path}' does not export {name}");

                call.Module.Exports[alias] = export;
            }
        }
        catch (Throw t)
        {
            exception = t;
        }

        if (exception is not null)
            yield return exception;
    }

    internal sealed record Export(INamedIdentifier Name, INamedIdentifier? Alias);
}