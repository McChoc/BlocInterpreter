using CmmInterpretor.Data;
using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Throw : IResult
    {
        public Value value;

        public Throw() => value = new Void();

        public Throw(string text) => value = new String(text);

        public Throw(Value value) => this.value = value;
    }
}
