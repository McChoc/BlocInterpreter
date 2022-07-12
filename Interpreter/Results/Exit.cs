using Bloc.Values;

namespace Bloc.Results
{
    public class Exit : Result
    {
        public Value Value { get; }

        public Exit() => Value = Void.Value;

        public Exit(Value value) => Value = value;
    }
}