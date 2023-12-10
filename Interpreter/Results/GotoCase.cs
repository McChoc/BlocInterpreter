using Bloc.Values.Core;

namespace Bloc.Results;

public sealed class GotoCase : IResult
{
    public Value Value { get; }

    public GotoCase(Value value) => Value = value;
}