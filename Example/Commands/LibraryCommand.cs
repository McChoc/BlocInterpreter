using System;
using System.Runtime.InteropServices;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using ConsoleApp.Utils;
using Void = Bloc.Values.Void;

namespace ConsoleApp.Commands;

public sealed class LibraryCommand : ICommandInfo
{
    private delegate int ExternCallback(string[] arguments);

    public string Name => "library";

    public string Description =>
        """
        library open <path>
        Opens a library at a given path and returns a libraryHadle.
        
        <library> |> library close
        Closes a library.

        <libraryHandle> |> library call <function> [arguments] ...
        Calls a function defined in a library with a given name with a list of arguments and returns the result.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length == 2 && args[0].ToLower() == "open")
        {
            var name = args[1];
            var libraryHandle = LibraryHelper.OpenLibrary(name);
            return new Extern(libraryHandle);
        }

        if (args.Length == 1 && args[0].ToLower() == "close")
        {
            if (input is not Extern @extern)
                throw new Throw("");

            if (@extern.Value is not IntPtr libraryHandle)
                throw new Throw("");

            LibraryHelper.CloseLibrary(libraryHandle);
            return Void.Value;
        }

        if (args.Length >= 2 && args[0].ToLower() == "call")
        {
            if (input is not Extern @extern)
                throw new Throw("");

            if (@extern.Value is not IntPtr libraryHandle)
                throw new Throw("");

            var name = args[1];
            var arguments = args[2..];

            var handle = LibraryHelper.GetFunction(libraryHandle, name);
            var function = Marshal.GetDelegateForFunctionPointer<ExternCallback>(handle);
            int result = function(arguments);

            return new Number(result);
        }

        throw new Throw("Invalid command");
    }
}