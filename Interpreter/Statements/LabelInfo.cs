namespace Bloc.Statements;

internal sealed record LabelInfo(int Index)
{
    internal int JumpCount { get; set; } = 0;
}