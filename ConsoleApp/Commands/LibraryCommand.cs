using System.Linq;
using System.Runtime.InteropServices;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;
using ConsoleApp.Utils;

namespace ConsoleApp.Commands;

public sealed class LibraryCommand : ICommandInfo
{
    private delegate int ExternCallback(string[] arguments);

    public string Name => "library";

    public string Description =>
        """
        library open <path>
        Opens a library at a given path and returns a extern library handle.
        
        library close <handle>
        Closes a library.

        library call <handle> <function> [arguments] ...
        Calls a function defined in a library with a given name with a list of arguments and returns the result.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        switch (args)
        {
            case [String command, String name] when command.Value.ToLower() == "open":
            {
                nint libraryHandle = LibraryHelper.OpenLibrary(name.Value);
                return new Extern(libraryHandle);
            }

            case [String command, Extern @extern] when command.Value.ToLower() == "close":
            {
                if (@extern.Value is not nint libraryHandle)
                    throw new Throw("Invalid command");

                LibraryHelper.CloseLibrary(libraryHandle);
                return Void.Value;
            }

            case [String command, Extern @extern, String name, ..] when command.Value.ToLower() == "call":
            {
                if (@extern.Value is not nint libraryHandle)
                    throw new Throw("Invalid command");

                var arguments = args[3..]
                        .Cast<String>()
                        .Select(x => x.Value)
                        .ToArray();

                nint functionHandle = LibraryHelper.GetFunction(libraryHandle, name.Value);
                var function = Marshal.GetDelegateForFunctionPointer<ExternCallback>(functionHandle);
                int result = function(arguments);

                return new Number(result);
            }
        }

        throw new Throw("Invalid command");
    }
}