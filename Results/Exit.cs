using CmmInterpretor.Data;
using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Exit : IResult
    {
        public Value value;

        public Exit() => value = new Void();

        public Exit(Value value) => this.value = value;
    }
}
