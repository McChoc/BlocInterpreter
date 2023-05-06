namespace Bloc.Core;

public struct EngineOptions : IEngineOptions
{
    public int StackLimit { get; set; } = 1000;
    public int LoopLimit { get; set; } = 1000;
    public int JumpLimit { get; set; } = 1000;
    public int HopLimit { get; set; } = 1000;

    public EngineOptions() { }
}