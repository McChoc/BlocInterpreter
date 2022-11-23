using Bloc.Values;

namespace Bloc.Results
{
    public sealed class Throw : Result
    {
        public Throw(Value value) => Value = value;

        public Throw(string text) => Value = new String(text);

        public Value Value { get; }
    }
}