using System;
using System.Runtime.InteropServices;

namespace ConsoleApp.Utils;

public static class LibraryHelper
{
    public static IntPtr OpenLibrary(string name)
    {
#if WINDOWS
        var libraryHandle = LoadLibrary(name);
#else
        var libraryHandle = dlopen(name, RTLD_LAZY);
#endif

        if (libraryHandle == IntPtr.Zero)
            throw new Exception($"Failed to open library: {name}");

        return libraryHandle;
    }

    public static IntPtr GetFunction(IntPtr library, string name)
    {
#if WINDOWS
        var functionHandle = GetProcAddress(library, name);
#else
        var functionHandle = dlsym(library, name);
#endif

        if (functionHandle == IntPtr.Zero)
            throw new Exception($"Failed to get function: {name}");

        return functionHandle;
    }

    public static void CloseLibrary(IntPtr library)
    {
#if WINDOWS
        FreeLibrary(library);
#else
        dlclose(library);
#endif
    }

#pragma warning disable CA2101
#if WINDOWS

    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string fileName);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr module, string procName);

    [DllImport("kernel32.dll")]
    private static extern bool FreeLibrary(IntPtr module);

#elif LINUX

    [DllImport("libdl.so")]
    private static extern IntPtr dlopen(string libraryName, int flags);

    [DllImport("libdl.so")]
    private static extern IntPtr dlsym(IntPtr library, string functionName);

    [DllImport("libdl.so")]
    private static extern int dlclose(IntPtr library);

#elif MACOS

    [DllImport("libSystem.dylib")]
    private static extern IntPtr dlopen(string libraryName, int flags);

    [DllImport("libSystem.dylib")]
    private static extern IntPtr dlsym(IntPtr library, string functionName);

    [DllImport("libSystem.dylib")]
    private static extern int dlclose(IntPtr library);

#endif
#pragma warning restore CA2101
}
