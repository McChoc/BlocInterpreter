using Bloc.Values;

namespace Bloc.Results
{
    public class Throw : Result
    {
        public Throw() => Value = Void.Value;

        public Throw(Value value) => Value = value;

        public Throw(string text) => Value = new String(text);

        public Value Value { get; }
    }
}