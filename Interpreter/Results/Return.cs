using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Results;

public sealed class Return : IResult
{
    public Value Value { get; }

    public Return() => Value = Void.Value;

    public Return(Value value) => Value = value;
}