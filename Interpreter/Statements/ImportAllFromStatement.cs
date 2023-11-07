using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class ImportAllFromStatement : Statement
{
    private readonly VariableScope _scope;

    internal IExpression ModulePathExpression { get; }

    internal ImportAllFromStatement(VariableScope scope, IExpression path)
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

            foreach (var (name, export) in module.Exports)
                call.Set(name, export, false, true, _scope);
        }
        catch (Throw t)
        {
            exception = t;
        }

        if (exception is not null)
            yield return exception;
    }
}