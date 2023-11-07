using System;
using System.Runtime.InteropServices;

namespace ConsoleApp.Utils;

public static class ConsoleTextStyler
{
    static ConsoleTextStyler()
    {
        var handle = GetStdHandle(-11);
        GetConsoleMode(handle, out var mode);
        SetConsoleMode(handle, mode | 0x4);
        Console.ResetColor();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out int mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

    public static void SetTextColor(ConsoleColor color)
    {
        Console.Write($"\x1b[38;5;{(byte)color}m");
    }
}