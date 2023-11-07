using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class ImportStatement : Statement
{
    private readonly VariableScope _scope;

    internal List<IExpression> ModulePathExpressions { get; } = new();

    internal ImportStatement(VariableScope scope)
    {
        _scope = scope;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        Throw? exception = null;

        try
        {
            foreach (var modulePathExpression in ModulePathExpressions)
            {
                string path = ImportHelper.ResolveModulePath(modulePathExpression, call);
                var module = ImportHelper.GetModule(path, call);

                foreach (var (name, export) in module.Exports)
                    call.Set(name, export, false, true, _scope);
            }
        }
        catch (Throw t)
        {
            exception = t;
        }

        if (exception is not null)
            yield return exception;
    }
}