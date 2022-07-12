using Bloc.Values;

namespace Bloc.Results
{
    public class Throw : Result
    {
        public Value Value {  get; }

        public Throw() => Value = Void.Value;

        public Throw(Value value) => Value = value;

        public Throw(string text) => Value = new String(text);
    }
}