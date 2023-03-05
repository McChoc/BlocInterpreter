using Bloc.Values;

namespace Bloc.Results;

public sealed class Return : IResult
{
    public Value Value { get; }

    public Return() => Value = Void.Value;

    public Return(Value value) => Value = value;
}