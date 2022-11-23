using Bloc.Values;

namespace Bloc.Results
{
    public sealed class Yield : Result
    {
        public Yield(Value value) => Value = value;

        public Value Value { get; }
    }
}