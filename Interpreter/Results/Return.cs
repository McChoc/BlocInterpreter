using Bloc.Values;

namespace Bloc.Results
{
    public sealed class Return : Result
    {
        public Return() => Value = Void.Value;

        public Return(Value value) => Value = value;

        public Value Value { get; }
    }
}