using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Throw : Result
    {
        public Value value;

        public Throw() => value = Void.Value;

        public Throw(Value value) => this.value = value;

        public Throw(string text) => value = new String(text);
    }
}
