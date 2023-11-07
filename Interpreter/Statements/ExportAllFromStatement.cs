using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;

namespace Bloc.Statements;

[Record]
internal sealed partial class ExportAllFromStatement : Statement
{
    internal IExpression ModulePathExpression { get; }

    internal ExportAllFromStatement(IExpression path)
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

            foreach (var (name, export) in module.Exports)
                call.Module.Exports[name] = export;
        }
        catch (Throw t)
        {
            exception = t;
        }

        if (exception is not null)
            yield return exception;
    }
}