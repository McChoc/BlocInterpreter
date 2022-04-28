using System;
using System.Runtime.InteropServices;

static class ConsoleColor
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out int mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int handle);

    static ConsoleColor()
    {
        var handle = GetStdHandle(-11);
        GetConsoleMode(handle, out int mode);
        SetConsoleMode(handle, mode | 0x4);
        Console.ResetColor();
    }

    public static void SetColor (byte color)
    {
        Console.Write($"\x1b[38;5;{color}m");
    }
}