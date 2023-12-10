using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace ConsoleApp.Commands;

public sealed class SetCommand : ICommandInfo
{
    public string Name => "set";

    public string Description =>
        """
        set <name> <value>
        <value> |> set <name>
        Creates a new variable with a specified name and value in the current scope.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length == 0)
            throw new Throw("'set' does not take 0 arguments.\nType '/help set' to see its usage");

        if (args.Length == 1)
        {
            if (input is Void)
                throw new Throw("The input was empty");

            if (args[0] is not String @string)
                throw new Throw("The name was not a string");

            call.Set(@string.Value, input.GetOrCopy(true), true, true, VariableScope.Local);

            return Void.Value;
        }

        if (args.Length == 2)
        {
            if (args[0] is not String @string)
                throw new Throw("The name was not a string");

            call.Set(@string.Value, args[1].GetOrCopy(true), true, true, VariableScope.Local);

            return Void.Value;
        }

        throw new Throw($"'set' does not take {args.Length} arguments.\nType '/help set' to see its usage");
    }
}