using System.Collections.Generic;
using System.IO;
using Bloc.Core;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Exceptions;
using Bloc.Values.Types;

namespace Bloc.Utils.Helpers;

internal static class ImportHelper
{
    internal static string ResolveModulePath(IExpression modulePathExpression, Call call)
    {
        var modulePathValue = modulePathExpression.Evaluate(call).Value;
        modulePathValue = ReferenceHelper.Resolve(modulePathValue, call.Engine.Options.HopLimit).Value;

        string modulePath = String.ImplicitCast(modulePathValue).Value;

        if (!modulePath.StartsWith("@"))
        {
            modulePath = Path.Combine(call.Module.Path, modulePath);
        }
        else
        {
            int index = modulePath.IndexOf('/');

            string alias = (index != -1)
                ? modulePath[1..index]
                : modulePath[1..];

            string remainingPath = (index != -1)
                ? modulePath[(index + 1)..]
                : "";

            if (call.Engine.Aliases.TryGetValue(alias, out string aliasedPath))
                modulePath = Path.Combine(aliasedPath, remainingPath);
            else
                throw new Throw($"Unknown path alias : {alias}");
        }

        return Path.GetFullPath(modulePath + ".bloc");
    }

    internal static Module GetModule(string path, Call call)
    {
        if (call.Engine.Modules.TryGetValue(path, out var module))
        {
            if (!module.Imported)
                throw new Throw($"Circular import");
        }
        else
        {
            if (!File.Exists(path))
                throw new Throw($"File {path} does not exists");

            string directoryPath = Path.GetDirectoryName(path);
            string code = File.ReadAllText(path);

            module = new Module(directoryPath, call.Engine);
            call.Engine.Modules.Add(path, module);

            List<Statement>? statements = null;
            SyntaxError? error = null;

            try
            {
                Engine.Compile(code, out var _, out statements);
            }
            catch (SyntaxError e)
            {
                error = e;
            }

            if (error != null)
                throw new Throw($"Syntax error : {error.Message}");

            var exception = call.Engine.Execute(statements!, module);

            if (exception != null)
                throw exception;

            module.Imported = true;
        }

        return module;
    }
}