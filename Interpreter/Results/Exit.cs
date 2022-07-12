using Bloc.Values;

namespace Bloc.Results
{
    public class Exit : Result
    {
        public Exit() => Value = Void.Value;

        public Exit(Value value) => Value = value;

        public Value Value { get; }
    }
}