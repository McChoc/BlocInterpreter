namespace Bloc.Core;

public interface IEngineOptions
{
    int StackLimit { get; }
    int LoopLimit { get; }
    int JumpLimit { get; }
    int HopLimit { get; }
}